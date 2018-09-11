﻿using System;

namespace MinimumSpanningTree
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Graph graph = new Graph();
            graph.ReadFromFile("1.dat");
            graph.PrintNodes();
            graph.FindMinimumSpanningTree();
            graph.FindMaximumSpanningTree();
            Console.ReadKey();
        }
    }
}