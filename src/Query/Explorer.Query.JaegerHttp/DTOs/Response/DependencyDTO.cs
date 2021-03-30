namespace Explorer.Query.JaegerHttp.DTOs.Response
{
    public class DependencyDTO
    {
        public string Parent { get; set; }
        public string Child { get; set; }
        public long CallCount { get; set; }
    }
}