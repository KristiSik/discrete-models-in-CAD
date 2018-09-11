namespace MinimumSpanningTree
{
    public class Edge
    {
        public int? Number { get; set; }
        public int Weight { get; set; }
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
    }
}