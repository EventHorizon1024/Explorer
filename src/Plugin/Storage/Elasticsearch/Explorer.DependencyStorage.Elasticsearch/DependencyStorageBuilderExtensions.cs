using System;
using Explorer.SpanStorage.Elasticsearch;
using Explorer.Storage;
using Explorer.Storage.Abstractions;
using Explorer.Storage.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Explorer.DependencyStorage.Elasticsearch
{
    public static class DependencyStorageBuilderExtensions
    {
        public static IStorageBuilder UseElasticsearch(this IStorageBuilder builder,
            Action<ElasticsearchOptions> configureOptions)
        {
            builder.Services.PostConfigure(configureOptions);
            builder.Services.AddHttpClient();
            builder.Services.TryAddSingleton<IIndexManager, IndexManager>();
            builder.Services.AddSingleton<IDependencyReader, ElasticsearchDependencyReader>();
            builder.Services.AddSingleton<IDependencyWriter, ElasticsearchDependencyWriter>();
            return builder;
        }
    }
}