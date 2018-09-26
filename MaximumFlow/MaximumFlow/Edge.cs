namespace MaximumFlow
{
    public class Edge
    {
        public int? Number { get; set; }
        public int Capacity { get; set; }               // maximum flow
        public Node StartNode { get; set; }
        public Node EndNode { get; set; }
        private int _flow;
        public int Flow                                 // used flow
        {
            get
            {
                return _flow;
            }
            set
            {
                _flow = value;
                if (_flow == Capacity)
                {
                    Full = true;
                }
            }
        }
        public bool Full { get; set; }
    }
}