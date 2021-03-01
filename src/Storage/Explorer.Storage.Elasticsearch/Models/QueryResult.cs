using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Explorer.Storage.Elasticsearch.Models
{
    public class HitWrapper<T>
    {
        public List<Hit<T>> Hits { get; set; }
    }

    public class Hit<T>
    {
        [JsonProperty("_source")] public T Source { get; set; }
    }

    public class Bucket
    {
        public string Key { get; set; }

        [JsonProperty("doc_count")] public int DocCount { get; set; }
    }

    public class Aggregation
    {
        public List<Bucket> Buckets { get; set; }

        public List<string> Keys => Buckets.ConvertAll(b => b.Key);
    }

    public class QueryResult<T>
    {
        public HitWrapper<T> Hits { get; set; }
        public Dictionary<string, Aggregation> Aggregations { get; set; }

        public List<T> Data => Hits?.Hits?.ConvertAll(h => h.Source);
    }

    public class MultiQueryResult<T>
    {
        public List<QueryResult<T>> Responses { get; set; }
    }
}