using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerianPath
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph graph = new Graph();
            graph.ReadFromFile("1.dat");
            graph.FindEulerianPath();
            Console.ReadKey();
        }
    }
}