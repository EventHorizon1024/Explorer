using System.Collections.Generic;

namespace Explorer.Models
{
    public class Process
    {
        public string ServiceName { get; set; }
        public Tag[] Tags { get; set; }
    }
}