using System;
using System.Collections.Generic;

namespace Explorer.Models
{
    public class SpanLog
    {
        public DateTimeOffset Timestamp { get; set; }
        
        public Tag[] Fields { get; set; }
    }
}