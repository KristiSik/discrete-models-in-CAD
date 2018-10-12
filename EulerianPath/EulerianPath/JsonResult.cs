using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EulerianPath
{
    [DataContract]
    class JsonResult
    {
        [DataMember]
        public List<Node> Nodes { get; set; }
        [DataMember]
        public List<Edge> Edges { get; set; }
        [DataMember]
        public List<Edge> EulerianPath { get; set; }
    }
}
