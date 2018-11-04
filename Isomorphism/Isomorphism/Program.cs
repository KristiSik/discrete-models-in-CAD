using System;

namespace Isomorphism
{
    class Program
    {
        static void Main(string[] args)
        {
            Graph g1 = new Graph();
            Graph g2 = new Graph();
            g1.ReadFromFile("1.dat");
            g2.ReadFromFile("2.dat");
            Console.WriteLine(g1.IsIsomorphic(g2));
            Console.ReadKey();
        }
    }
}
