namespace Explorer.Query.JaegerHttp.DTOs.Response
{
    public class DependencyLinkDTO
    {
        public string Parent { get; set; }
        public string Child { get; set; }
        public long CallCount { get; set; }
    }
}