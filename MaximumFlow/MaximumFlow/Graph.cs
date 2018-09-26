using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaximumFlow.Services;

namespace MaximumFlow
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

        public void AddEdge(int edgeNumber, int edgeCapacity, string startNodeName, string endNodeName)
        {
            Edge newEdge = new Edge() { Capacity = edgeCapacity, Number = edgeNumber };
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
                    if (!int.TryParse(line[0], out var edgeCapacity))
                    {
                        throw new Exception();
                    }

                    string startNodeName = line[1];
                    string endNodeName = line[2];
                    AddEdge(++EdgeNumber, edgeCapacity, startNodeName, endNodeName);
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
                    n.InEdges.ForEach(e => Console.WriteLine($"\t\t{e.Number} ({e.Capacity})"));
                }
                if (n.OutEdges.Count > 0)
                {
                    Console.WriteLine("\tOutEdges:");
                    n.OutEdges.ForEach(e => Console.WriteLine($"\t\t{e.Number} ({e.Capacity})"));
                }
            });
        }

        public void FindMaximumFlow(string startNodeName, string endNodeName)
        {
            List<Edge> path = new List<Edge>();
            bool exit = false;
            do
            {
                Node currentNode = Nodes.FirstOrDefault(n => n.Name == startNodeName);
                Node startNode = currentNode;
                Edge previousEdge = new Edge();
                while (currentNode.Name != endNodeName)
                {
                    Edge edge = currentNode.OutEdges.FirstOrDefault(e => !e.Full);
                    if (edge == null)
                    {
                        if (currentNode.Name == startNodeName)
                        {
                            exit = true;
                            break;
                        }
                        else
                        {
                            previousEdge.Full = true;
                            currentNode = startNode;
                        }
                    }
                    else
                    {
                        path.Add(edge);
                        currentNode = edge.EndNode;
                    }
                    previousEdge = edge;
                }
                if (!exit)
                {
                    CalculateMinCapacity(path);
                    InfoService.DisplayFlow(path);
                }
                path.Clear();
            } while (!exit);
            InfoService.DisplayEdges(Edges);
            Console.WriteLine($"Maximum flow: {CalculateMaximumFlow(endNodeName)}");
        }

        private void CalculateMinCapacity(List<Edge> path)
        {
            int minCapacity = path.Min(e => e.Capacity - e.Flow);
            path.ForEach(e => e.Flow = e.Flow + minCapacity);
        }
        private int CalculateMaximumFlow(string endNodeName)
        {
            return Nodes.First(n => n.Name == endNodeName).InEdges.Sum(e => e.Flow);
        }
    }
}