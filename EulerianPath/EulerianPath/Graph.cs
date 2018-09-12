using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EulerianPath.Services;

namespace EulerianPath
{
    public class Graph
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        public List<Edge> UnusedEdges { get; set; }
        public List<Edge> UsedEdges { get; set; }
        public int[][] AdjacencyMatrix { get; set; }
        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public void AddEdge(int edgeNumber, int edgeWeight, string startNodeName, string endNodeName)
        {
            Edge newEdge = new Edge() { Weight = edgeWeight, Number = edgeNumber };
            Node startNode = Nodes.FirstOrDefault(n => n.Name == startNodeName);
            Node endNode = Nodes.FirstOrDefault(n => n.Name == endNodeName);
            if (startNode == null)
            {
                startNode = new Node()
                {
                    Name = startNodeName
                };
                Nodes.Add(startNode);
            }
            if (endNode == null)
            {
                endNode = new Node()
                {
                    Name = endNodeName
                };
                Nodes.Add(endNode);
            }
            endNode.InEdges.Add(newEdge);
            startNode.OutEdges.Add(newEdge);
            newEdge.StartNode = startNode;
            newEdge.EndNode = endNode;
            Edges.Add(newEdge);
        }

        public void ReadFromFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(fs);
            int edgeNumber = 0;
            while (sr.Peek() >= 0)
            {
                try
                {
                    var line = sr.ReadLine()?.Split(' ');
                    if (line == null)
                        throw new Exception();
                    if (line[0] == "#")
                        continue;
                    if (!int.TryParse(line[0], out var edgeWeight))
                    {
                        throw new Exception();
                    }

                    string startNodeName = line[1];
                    string endNodeName = line[2];
                    AddEdge(++edgeNumber, edgeWeight, startNodeName, endNodeName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed reading from file.");
                }
            }
            sr.Close();
            fs.Close();
        }

        public void PrintNodes()
        {
            Nodes.ForEach(n =>
            {
                Console.WriteLine($"{n.Name}:");
                if (n.InEdges.Count > 0)
                {
                    Console.WriteLine("\tInEdges:");
                    n.InEdges.ForEach(e => Console.WriteLine($"\t\t{e.Number} ({e.Weight})"));
                }
                if (n.OutEdges.Count > 0)
                {
                    Console.WriteLine("\tOutEdges:");
                    n.OutEdges.ForEach(e => Console.WriteLine($"\t\t{e.Number} ({e.Weight})"));
                }
            });
        }

        private List<Tuple<Node, Node>> FindPair(List<Node> nodes)
        {
            Node firstNode = nodes.First();
            int minWeight = int.MaxValue;
            List<Tuple<Node, Node>> bestPairs = new List<Tuple<Node, Node>>();
            Tuple<Node, Node> currentPair;
            if (nodes.Count == 2)
            {
                return new List<Tuple<Node, Node>>() { new Tuple<Node, Node>(firstNode, nodes.Last()) };
            }
            foreach (var node in nodes.Skip(1))
            {
                currentPair = new Tuple<Node, Node>(firstNode, node);
                List<Tuple<Node, Node>> newPair = FindPair(nodes.Except(new List<Node>() {node, firstNode}).ToList());
                int weight =
                    CalculateWeightOfPairs(newPair);
                if (weight < minWeight)
                {
                    minWeight = weight;
                    bestPairs = new List<Tuple<Node, Node>>() { currentPair };
                    bestPairs.AddRange(newPair);
                }
            }

            return bestPairs;
        }

        private int CalculateWeightOfPairs(List<Tuple<Node, Node>> pairs)
        {
            int sum = 0;
            foreach (var pair in pairs)
            {
                sum += AdjacencyMatrix[Nodes.IndexOf(pair.Item1)][Nodes.IndexOf(pair.Item2)];
            }

            return sum;
        }
        public void FindEulerianPath()
        {
            if (!IsEvenDegree())
            {
                Console.WriteLine("Graph has vertex with odd degree");
                MakeAdjacencyMatrix();
                FindMinimumDistances();
                var oddDegreeNodes = Nodes.FindAll(n => (n.InEdges.Count + n.OutEdges.Count) % 2 != 0);
                List<Tuple<Node, Node>> pairs = FindPair(oddDegreeNodes);
                CreateEdgesBetweenOddDegreeVertexes(pairs);
                Console.WriteLine(CalculateWeightOfPairs(pairs));
            }
            UsedEdges = new List<Edge>();
            UnusedEdges = new List<Edge>(Edges);
            //starting from first node in Nodes list
            var startingEdge = Nodes.First().InEdges.Count > 0 ? Nodes.First().InEdges.First() : Nodes.First().OutEdges.First();
            List<Edge> united = new List<Edge>();
            united.AddRange(FindCycle(Nodes.First()));
            while (UnusedEdges.Count > 0)
            {
                united = UniteCycles(united, FindCycle(UnusedEdges.First().StartNode));
            }
            united.ForEach(u => Console.WriteLine($"{u.StartNode.Name}, {u.EndNode.Name}"));
        }

        private void CreateEdgesBetweenOddDegreeVertexes(List<Tuple<Node, Node>> pairs)
        {
            int number = 1000;
            foreach (var pair in pairs)
            {
                Edges.Add(new Edge()
                {
                    StartNode = pair.Item1,
                    EndNode = pair.Item2,
                    Number = number++,
                    Weight = AdjacencyMatrix[Nodes.IndexOf(pair.Item1)][Nodes.IndexOf(pair.Item2)]
                });
            }
        }
        private bool IsEvenDegree()
        {
            foreach (var node in Nodes)
            {
                if ((node.InEdges.Count + node.OutEdges.Count) % 2 != 0)
                {
                    return false;
                }
            }
            return true;
        }

        private List<Edge> FindCycle(Node startingNode)
        {
            List<Edge> cycle = new List<Edge>();
            Edge nextEdge;
            Node lastNode = startingNode;
            do
            {
                nextEdge = FindNextEdge(lastNode);
                cycle.Add(nextEdge);
                UnusedEdges.RemoveAll(ue => ue.Number == nextEdge.Number);
                UsedEdges.Add(nextEdge);
                lastNode = lastNode == nextEdge.StartNode ? nextEdge.EndNode : nextEdge.StartNode;
            } while (lastNode != startingNode);
            return cycle;
        }

        private List<Edge> UniteCycles(List<Edge> firstCycle, List<Edge> secondCycle)
        {
            List<Edge> unitedCycles = new List<Edge>();
            bool united = false;
            foreach (var edgeFromFirstCycle in firstCycle)
            {
                unitedCycles.Add(edgeFromFirstCycle);
                if (united) continue;
                if (edgeFromFirstCycle.StartNode == secondCycle.First().EndNode ||
                    edgeFromFirstCycle.EndNode == secondCycle.First().StartNode)
                {
                    foreach (var edgeFromSecondCycle in secondCycle)
                    {
                        unitedCycles.Add(edgeFromSecondCycle);
                    }

                    united = true;
                }
            }
            return unitedCycles;
        }
        private Edge FindNextEdge(Node currentNode)
        {
            var bestNodeName = UnusedEdges.FindAll(e => e.StartNode == currentNode || e.EndNode == currentNode)
                .Min(e => (e.StartNode == currentNode) ? e.EndNode.Name : e.StartNode.Name);
            Edge bestEdge = UnusedEdges.FirstOrDefault(e => (e.StartNode == currentNode && e.EndNode.Name == bestNodeName) || (e.EndNode == currentNode && e.StartNode.Name == bestNodeName));
            bestEdge = FixEdgeDirection(bestEdge, currentNode);
            return bestEdge;
        }

        private Edge FixEdgeDirection(Edge edge, Node startNode)
        {
            if (edge.StartNode != startNode)
            {
                edge.EndNode = edge.StartNode;
                edge.StartNode = startNode;
            }
            return edge;
        }
        private void MakeAdjacencyMatrix()
        {
            AdjacencyMatrix = new int[Nodes.Count][];
            for (int i = 0; i < Nodes.Count; i++)
            {
                AdjacencyMatrix[i] = new int[Nodes.Count];
            }
            foreach (var edge in Edges)
            {
                AdjacencyMatrix[Nodes.IndexOf(edge.StartNode)][Nodes.IndexOf(edge.EndNode)] = edge.Weight;
                AdjacencyMatrix[Nodes.IndexOf(edge.EndNode)][Nodes.IndexOf(edge.StartNode)] = edge.Weight;
            }
        }
        private void FindMinimumDistances()
        {
            for (int k = 0; k < Nodes.Count; ++k)
            {
                for (int i = 0; i < Nodes.Count; ++i)
                {
                    for (int j = 0; j < Nodes.Count; ++j)
                    {
                        //0 - no edge between two nodes
                        if (i!=j && AdjacencyMatrix[i][k] != 0 && AdjacencyMatrix[k][j] != 0)
                        {
                            AdjacencyMatrix[i][j] = Math.Min(AdjacencyMatrix[i][j], AdjacencyMatrix[i][k] + AdjacencyMatrix[k][j]);
                        }
                    }
                }
            }
            for (int i = 0; i < Nodes.Count; i++)
            {
                for (int j = 0; j < Nodes.Count; j++)
                {
                    Console.Write(AdjacencyMatrix[i][j] + " ");
                }

                Console.WriteLine();
            }
        }
    }
}