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
        public int[][] RoutesMatrix { get; set; }
        public int EdgeNumber { get; set; }
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
                    AddEdge(++EdgeNumber, edgeWeight, startNodeName, endNodeName);
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
            }

            UsedEdges = new List<Edge>();
            UnusedEdges = new List<Edge>(Edges);
            //starting from first node in Nodes list
            var startingEdge = Nodes.First().InEdges.Count > 0
                ? Nodes.First().InEdges.First()
                : Nodes.First().OutEdges.First();
            List<Edge> united = new List<Edge>();
            united.AddRange(FindCycle(Nodes.First()));
            while (UnusedEdges.Count > 0)
            {
                united = UniteCycles(united, FindCycle(UnusedEdges.First().StartNode));
            }

            united.ForEach(u => Console.WriteLine($"{u.StartNode.Name}, {u.EndNode.Name} ({u.Weight})"));
            Console.WriteLine(united.Sum(u => u.Weight));
        }

        private void CreateEdgesBetweenOddDegreeVertexes(List<Tuple<Node, Node>> pairs)
        {
            foreach (var pair in pairs)
            {
                CreateEdgeBetweenTwoNodes(Nodes.IndexOf(pair.Item1), Nodes.IndexOf(pair.Item2));
            }
        }
        private void CreateEdgeBetweenTwoNodes(int firstNodeIndex, int secondNodeIndex)
        {
            int nextNodeIndex = RoutesMatrix[firstNodeIndex][secondNodeIndex];
            if (nextNodeIndex == -1)
            {
                Edges.Add(new Edge()
                {
                    StartNode = Nodes[firstNodeIndex],
                    EndNode = Nodes[secondNodeIndex],
                    Number = ++EdgeNumber,
                    Weight = AdjacencyMatrix[firstNodeIndex][secondNodeIndex]
                });
            }
            else
            {
                CreateEdgeBetweenTwoNodes(firstNodeIndex, nextNodeIndex);
                CreateEdgeBetweenTwoNodes(nextNodeIndex, nextNodeIndex);
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
            RoutesMatrix = new int[Nodes.Count][];
            for (int i = 0; i < Nodes.Count; i++)
            {
                RoutesMatrix[i] = new int[Nodes.Count];
                AdjacencyMatrix[i] = new int[Nodes.Count];
                for (int j = 0; j < Nodes.Count; j++)
                {
                    AdjacencyMatrix[i][j] = -1;
                    RoutesMatrix[i][j] = -1;
                }
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
                        if (i == j) continue;
                        //-1 - no edge between two nodes in Adjacency matrix, -1 - there is no one node between two nodes in Routes matrix
                        if (AdjacencyMatrix[i][k] != -1 && AdjacencyMatrix[k][j] != -1)
                        {
                            if (AdjacencyMatrix[i][j] == -1)
                            {
                                RoutesMatrix[i][j] = k;
                                AdjacencyMatrix[i][j] = AdjacencyMatrix[i][k] + AdjacencyMatrix[k][j];
                            } else if (AdjacencyMatrix[i][k] + AdjacencyMatrix[k][j] < AdjacencyMatrix[i][j])
                            {
                                RoutesMatrix[i][j] = k;
                                AdjacencyMatrix[i][j] = AdjacencyMatrix[i][k] + AdjacencyMatrix[k][j];
                            }
                        }
                    }
                }
            }
            //Console.Write("  ");
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    Console.Write(Nodes[i].Name + " ");
            //}
            //Console.WriteLine();
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    for (int j = 0; j < Nodes.Count; j++)
            //    {
            //        if (j == 0)
            //        {
            //            Console.Write(Nodes[i].Name + " ");
            //        }
            //        Console.Write(AdjacencyMatrix[i][j] + " ");
            //    }

            //    Console.WriteLine();
            //}

            //Console.Write("\t");
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    Console.Write($"{Nodes[i].Name} {i}\t");
            //}
            //Console.WriteLine();
            //for (int i = 0; i < Nodes.Count; i++)
            //{
            //    for (int j = 0; j < Nodes.Count; j++)
            //    {
            //        if (j == 0)
            //        {
            //            Console.Write(Nodes[i].Name + " " + i + "\t");
            //        }
            //        Console.Write(RoutesMatrix[i][j] + "\t");
            //    }

            //    Console.WriteLine();
            //}
        }
    }
}