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

        public void FindEulerianPath()
        {
            if (!IsEvenDegree())
            {
                Console.WriteLine("Graph has vertex with odd degree");
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
    }
}
