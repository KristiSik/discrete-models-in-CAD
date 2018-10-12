using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MinimumSpanningTree
{
    [DataContract]
    class JsonResult
    {
        [DataMember]
        public List<Node> Nodes { get; set; }
        [DataMember]
        public List<Edge> Edges { get; set; }
        [DataMember]
        public List<Edge> MinimumSpanningTree { get; set; }
        [DataMember]
        public List<Edge> MaximumSpanningTree { get; set; }
    }
}
