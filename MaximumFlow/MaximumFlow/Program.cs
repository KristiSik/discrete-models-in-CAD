using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaximumFlow
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph g = new Graph();
            g.ReadFromFile("1.dat");
            g.FindMaximumFlow("a", "f");
            Console.ReadKey();
        }
    }
}