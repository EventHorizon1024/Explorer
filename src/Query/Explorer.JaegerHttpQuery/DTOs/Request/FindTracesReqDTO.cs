using System;
using System.Collections.Generic;

namespace Explorer.JaegerHttpQuery.DTOs.Request
{
    public class FindTracesReqDto
    {
        public string[] TraceID { get; set; }

        public string Service { get; set; }

        public string Operation { get; set; }

        public string Tags { get; set; }

        public string LookBack { get; set; }

        public long? Start { get; set; }

        public long? End { get; set; }

        public string MinDuration { get; set; }

        public string MaxDuration { get; set; }
        
        public int Limit { get; set; }
    }
}