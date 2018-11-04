using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isomorphism
{
    public class Graph
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
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

        public bool IsIsomorphic(Graph graph)
        {
            // 1: Comparing number of nodes
            if (graph.Nodes.Count != Nodes.Count)
            {
                return false;
            }
            
            // 2: Comparing number of edges
            if (graph.Edges.Count != Edges.Count)
            {
                return false;
            }

            // 3: Comparing number of edges which has each node
            // Ordering nodes
            List<Node> orderedNodesFromFirstGraph = Nodes.OrderBy(n => n.InEdges.Count + n.OutEdges.Count).ToList();
            List<Node> orderedNodesFromSecondGraph = graph.Nodes.OrderBy(n => n.InEdges.Count + n.OutEdges.Count).ToList();
            // Grouping ordered nodes
            List<IGrouping<int, Node>> groupedNodesFromFirstGraph = orderedNodesFromFirstGraph
                .GroupBy(n => n.InEdges.Count + n.OutEdges.Count)
                .OrderBy(g => g.Key).ToList();
            List<IGrouping<int, Node>> groupedNodesFromSecondGraph = orderedNodesFromSecondGraph
                .GroupBy(n => n.InEdges.Count + n.OutEdges.Count)
                .OrderBy(g => g.Key).ToList();
            if (groupedNodesFromFirstGraph.Count != groupedNodesFromSecondGraph.Count)
            {
                return false;
            }
            int groupWithUniqueNodesIndex = 0, countOfUniqueNodes = Nodes.Count;
            for(int i = 0; i < groupedNodesFromFirstGraph.Count; i++)
            {
                if (groupedNodesFromFirstGraph[i].Key != groupedNodesFromSecondGraph[i].Key 
                    || groupedNodesFromFirstGraph[i].Count() != groupedNodesFromSecondGraph[i].Count())
                {
                    return false;
                }
                if (groupedNodesFromFirstGraph[i].Count() < countOfUniqueNodes)
                {
                    countOfUniqueNodes = groupedNodesFromFirstGraph[i].Count();
                    groupWithUniqueNodesIndex = i;
                }
            }
            
            // 4: Check for connectivity
            int numberOfConnectedNodesInFirstGraph = DepthFirstTraversal(this, Nodes[0]).ToList().Count;
            int numberOfConnectedNodesInSecondGraph = DepthFirstTraversal(graph, graph.Nodes[0]).ToList().Count;
            if (numberOfConnectedNodesInFirstGraph != Nodes.Count || numberOfConnectedNodesInSecondGraph != graph.Nodes.Count)
            {
                return false;
            }

            // 5: Hanging nodes algorithm
            foreach(Node firstNode in groupedNodesFromFirstGraph[groupWithUniqueNodesIndex])
            {
                foreach(Node secondNode in groupedNodesFromSecondGraph[groupWithUniqueNodesIndex])
                {
                    if (HangNodes(this, firstNode, graph, secondNode))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HangNodes(Graph graph1, Node startNode1, Graph graph2, Node startNode2)
        {
            // Caclulating number of nodes and edges on each level
            Tuple<List<int>, List<int>> t1 = GraphLevels(graph1, startNode1);
            Tuple<List<int>, List<int>> t2 = GraphLevels(graph2, startNode2);
            // Comparing number of levels
            if (t1.Item1.Count != t2.Item1.Count)
            {
                return false;
            }
            // Comparing number of nodes and edges on each level
            for (int i = 0; i < t1.Item1.Count; i++)
            {
                if (t1.Item1[i] != t2.Item1[i] || t1.Item2[i] != t2.Item2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private Tuple<List<int>, List<int>> GraphLevels(Graph graph, Node startNode)
        {
            List<int> numberOfEdges = new List<int>();
            List<int> numberOfNodes = new List<int>();
            var visited = new HashSet<Node>();     // visited nodes of graph
            var stack = new Stack<Node>();         // current level of graph
            var stackNext = new Stack<Node>();     // next level of graph
            stackNext.Push(startNode);
            while(stackNext.Count > 0)
            {
                numberOfNodes.Add(stackNext.Count);
                while (stackNext.Count > 0)
                {
                    Node node = stackNext.Pop();
                    stack.Push(node);
                    visited.Add(node);
                }
                int numberOfEdgesOnLevel = 0;
                foreach(Node node in stack)
                {
                    node.InEdges
                        .ForEach(e => {
                            if (stack.ToList().Any(n => n.OutEdges.Contains(e)))
                            {
                                numberOfEdgesOnLevel++;
                            }
                        });
                }
                numberOfEdges.Add(numberOfEdgesOnLevel);
                while (stack.Count > 0)
                {
                    Node currentNode = stack.Pop();
                    currentNode.InEdges
                        .Select(e => e.EndNode == currentNode ? e.StartNode : e.EndNode)
                        .Union(
                            currentNode.OutEdges
                            .Select(e => e.EndNode == currentNode ? e.StartNode : e.EndNode))
                        .Where(n => !visited.Contains(n) && !stackNext.Contains(n))
                        .ToList()
                        .ForEach(n => stackNext.Push(n));
                }
            };
            return new Tuple<List<int>, List<int>>(numberOfEdges, numberOfNodes);
        }

        private IEnumerable<Node> DepthFirstTraversal(Graph graph, Node start)
        {
            var visited = new HashSet<Node>();
            var stack = new Stack<Node>();
            stack.Push(start);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (!visited.Add(current))
                    continue;
                yield return current;
                var neighbours = current.InEdges.Select(e => e.EndNode == current?e.StartNode:e.EndNode)
                    .Union(current.OutEdges.Select(e => e.EndNode == current ? e.StartNode : e.EndNode)).ToList()
                                      .Where(n => !visited.Contains(n));
                foreach (var neighbour in neighbours)
                    stack.Push(neighbour);
            }
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
    }
}