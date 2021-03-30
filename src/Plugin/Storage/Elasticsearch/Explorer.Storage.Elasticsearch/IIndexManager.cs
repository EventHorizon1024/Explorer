using System;
using System.Threading.Tasks;

namespace Explorer.Storage.Elasticsearch
{
    public interface IIndexManager
    {
        ValueTask CreateIndexIfAbsentAsync(string indexName, Func<string> mappingProvider);
    }
}