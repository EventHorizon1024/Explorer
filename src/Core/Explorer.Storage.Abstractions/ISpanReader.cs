using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Storage.Abstractions
{
    public interface ISpanReader
    {
        Task<string[]> GetSeriesAsync();
        
        Task<string[]> GetOperationsAsync(string serviceName);

        Task<Trace[]> FindTracesAsync(TraceQueryParameters query);
        Task<Trace[]> FindTracesAsync(string[] traceIDs, long? startTime = null, long? endTime = null);
    }

    public class TraceQueryParameters
    {
        public string ServiceName { get; set; }

        public string OperationName { get; set; }

        public Dictionary<string, object> Tags { get; set; }

        public long? StartTimeMin { get; set; }

        public long? StartTimeMax { get; set; }

        public long? DurationMin { get; set; }

        public long? DurationMax { get; set; }

        public int NumTraces { get; set; }
    }
}