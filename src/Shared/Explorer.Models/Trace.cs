namespace Explorer.Models
{
    public class Trace
    {
        public string TraceID { get; set; }

        public Span[] Spans { get; set; }
    }
}