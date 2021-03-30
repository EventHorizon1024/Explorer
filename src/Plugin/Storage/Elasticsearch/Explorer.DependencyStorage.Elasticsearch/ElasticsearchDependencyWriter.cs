using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.Storage.Abstractions;

namespace Explorer.DependencyStorage.Elasticsearch
{
    public class ElasticsearchDependencyWriter : IDependencyWriter
    {
        public Task WriteAsync(IEnumerable<Dependency> dependencies)
        {
            throw new NotImplementedException();
        }
    }
}