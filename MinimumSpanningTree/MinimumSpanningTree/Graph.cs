using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using MinimumSpanningTree.Services;

namespace MinimumSpanningTree
{
    public class Graph
    {
        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }
        private List<Edge> minimumSpanningTree = new List<Edge>();
        private List<Edge> maximumSpanningTree = new List<Edge>();
        
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
        public void FindMinimumSpanningTree()
        {
            List<Node> usedNodes = new List<Node> {Nodes.First()};
            while (usedNodes.Count < Nodes.Count)
            {
                Edge edgeWithMinimumWeight = null;
                foreach (var node in usedNodes)
                {
                    node.InEdges.Union(node.OutEdges).ToList().ForEach(e =>
                    {
                        if (edgeWithMinimumWeight == null)
                        {
                            if (!(usedNodes.Any(un => un.Name == e.EndNode.Name) &&
                                  usedNodes.Any(un => un.Name == e.StartNode.Name)))
                            {
                                edgeWithMinimumWeight = e;
                            }
                        }
                        else
                        {
                            if (edgeWithMinimumWeight.Weight > e.Weight && !(usedNodes.Any(un => un.Name == e.EndNode.Name) && usedNodes.Any(un => un.Name == e.StartNode.Name)))
                            {
                                edgeWithMinimumWeight = e;
                            }
                        }
                    });
                }

                if (usedNodes.Any(un => un.Name == edgeWithMinimumWeight.EndNode.Name))
                {
                    usedNodes.Add(Nodes.First(n => n.OutEdges.Contains(edgeWithMinimumWeight)));
                }
                else
                if (usedNodes.Any(un => un.Name == edgeWithMinimumWeight.StartNode.Name))
                {
                    usedNodes.Add(Nodes.First(n => n.InEdges.Contains(edgeWithMinimumWeight)));
                }
                minimumSpanningTree.Add(edgeWithMinimumWeight);
            }
            InfoService.DisplayEdges(minimumSpanningTree);
        }

        public void FindMaximumSpanningTree()
        {
            List<Node> usedNodes = new List<Node> { Nodes.First() };
            while (usedNodes.Count < Nodes.Count)
            {
                Edge edgeWithMaximumWeight = null;
                foreach (var node in usedNodes)
                {
                    node.InEdges.Union(node.OutEdges).ToList().ForEach(e =>
                    {
                        if (edgeWithMaximumWeight == null)
                        {
                            if (!(usedNodes.Any(un => un.Name == e.EndNode.Name) &&
                                  usedNodes.Any(un => un.Name == e.StartNode.Name)))
                            {
                                edgeWithMaximumWeight = e;
                            }
                        }
                        else
                        {
                            if (edgeWithMaximumWeight.Weight < e.Weight && !(usedNodes.Any(un => un.Name == e.EndNode.Name) && usedNodes.Any(un => un.Name == e.StartNode.Name)))
                            {
                                edgeWithMaximumWeight = e;
                            }
                        }
                    });
                }

                if (usedNodes.Any(un => un.Name == edgeWithMaximumWeight.EndNode.Name))
                {
                    usedNodes.Add(Nodes.First(n => n.OutEdges.Contains(edgeWithMaximumWeight)));
                }
                else
                if (usedNodes.Any(un => un.Name == edgeWithMaximumWeight.StartNode.Name))
                {
                    usedNodes.Add(Nodes.First(n => n.InEdges.Contains(edgeWithMaximumWeight)));
                }
                maximumSpanningTree.Add(edgeWithMaximumWeight);
            }
            InfoService.DisplayEdges(maximumSpanningTree);
        }
        public void GenerateJsonFile()
        {
            JsonResult jr = new JsonResult();
            Nodes.ForEach(n => {
                n.InEdges.Clear();
                n.OutEdges.Clear();
            });
            jr.Edges = Edges;
            jr.Nodes = Nodes;
            jr.MinimumSpanningTree = minimumSpanningTree;
            jr.MaximumSpanningTree = maximumSpanningTree;
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            using (StreamWriter sw = new StreamWriter("result.json"))
            {
                sw.Write(jsonSerializer.Serialize(jr));
            }
        }
        public void RunWebPage()
        {
            System.Diagnostics.Process.Start("CMD.exe", "/C reload -b");
        }
    }
}
