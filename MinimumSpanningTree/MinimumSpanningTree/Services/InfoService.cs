using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimumSpanningTree.Services
{
    public static class InfoService
    {
        public static void DisplayEdges(List<Edge> edges)
        {
            edges.ForEach(e =>
            {
                Console.Write($"{e.Number} ({e.Weight})");
                if (e != edges.Last())
                {
                    Console.Write(", ");
                }
            });
            Console.WriteLine();
        }
    }
}