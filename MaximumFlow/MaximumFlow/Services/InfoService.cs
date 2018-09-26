using System;
using System.Collections.Generic;
using System.Linq;

namespace MaximumFlow.Services
{
    public static class InfoService
    {
        private static int flowNumber;
        public static void DisplayEdges(List<Edge> edges)
        {
            Console.WriteLine("\nGraph edges:");
            edges.ForEach(e =>
            {
                Console.Write($"{e.StartNode.Name} -> {e.EndNode.Name} ({e.Flow}/{e.Capacity})");
                if (e != edges.Last())
                {
                    Console.Write(", ");
                }
            });
            Console.WriteLine();
        }
        public static void DisplayFlow(List<Edge> edges)
        {
            Console.WriteLine($"Flow {++flowNumber}:");
            edges.ForEach(e =>
            {
                Console.Write($"{e.StartNode.Name} -> {e.EndNode.Name} ({e.Flow}/{e.Capacity})");
                if (e != edges.Last())
                {
                    Console.Write(", ");
                }
            });
            Console.WriteLine();
        }
    }
}