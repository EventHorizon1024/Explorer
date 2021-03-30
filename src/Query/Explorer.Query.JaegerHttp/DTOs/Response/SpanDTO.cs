using System;
using Explorer.Models;

namespace Explorer.Query.JaegerHttp.DTOs.Response
{
    public class SpanDTO
    {
        public string TraceID { get; set; }

        public string SpanID { get; set; }

        public string OperationName { get; set; }

        public uint Flags { get; set; }

        public long StartTime { get; set; }

        public long Duration { get; set; }
        
        public string ProcessID { get; set; }

        public SpanReferenceDTO[] References { get; set; }

        public Tag[] Tags { get; set; }

        public SpanLog[] Logs { get; set; }
    }
}