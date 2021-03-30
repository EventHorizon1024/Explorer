namespace Explorer.Models
{
    public class Dependency
    {
        public string Parent { get; set; }

        public string Child { get; set; }

        public long CallCount { get; set; } 
    }
}