using System.Collections.Generic;
using Explorer.Models;

namespace Explorer.Query.JaegerHttp.DTOs.Response
{
    public class TraceDTO
    {
        public string TraceID { get; set; }

        public Dictionary<string, ProcessDTO> Processes { get; set; }

        public SpanDTO[] Spans { get; set; }
    }
}