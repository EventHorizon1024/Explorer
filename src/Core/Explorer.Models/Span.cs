using System;
using System.Collections.Generic;

namespace Explorer.Models
{
    public class Span
    {
        public string TraceID { get; set; }

        public string SpanID { get; set; }

        public string OperationName { get; set; }

        public uint Flags { get; set; }

        /// <summary>
        /// UnixTimeMicroseconds
        /// </summary>
        public long StartTime { get; set; }

        /// <summary>
        /// UnixTimeMicroseconds
        /// </summary>
        public long Duration { get; set; }

        public Process Process { get; set; }

        public SpanReference[] References { get; set; }

        public Tag[] Tags { get; set; }

        public SpanLog[] Logs { get; set; }

        public string[] Warnings { get; set; }
    }
}