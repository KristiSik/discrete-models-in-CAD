using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravellingSalesmanProblem
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph g = new Graph();
            g.ReadAdjacencyMatrix("1.dat");
            g.TravellingSalesmanProblem(g.AdjacencyMatrix, 0, new List<Graph.Edge>());
            Console.ReadKey();
        }
    }
}