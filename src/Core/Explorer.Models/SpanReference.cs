namespace Explorer.Models
{
    public class SpanReference
    {
        public string TraceID { get; set; }

        public string SpanID { get; set; }

        public SpanRefType RefType { get; set; }
    }
}