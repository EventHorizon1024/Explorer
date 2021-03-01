using Explorer.Models;

namespace Explorer.JaegerHttpQuery.DTOs.Response
{
    public class ProcessDTO
    {
        public string ProcessID { get; set; }
        
        public string ServiceName { get; set; }

        public Tag[] Tags { get; set; }
    }
}