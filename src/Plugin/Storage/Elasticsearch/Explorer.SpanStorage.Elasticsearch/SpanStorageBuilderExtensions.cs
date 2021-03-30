using System;
using Explorer.Storage;
using Explorer.Storage.Abstractions;
using Explorer.Storage.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Explorer.SpanStorage.Elasticsearch
{
    public static class SpanStorageBuilderExtensions
    {
        public static ISpanStorageBuilder UseElasticsearch(this ISpanStorageBuilder builder, Action<ElasticsearchOptions> 
        configureOptions)
        {
            builder.Services.PostConfigure(configureOptions);
            builder.Services.AddHttpClient();
            builder.Services.TryAddSingleton<IIndexManager, IndexManager>();
            builder.Services.AddSingleton<ISpanWriter, ElasticsearchSpanWriter>();
            builder.Services.AddSingleton<ISpanReader, ElasticsearchSpanReader>();
            return builder;
        }
    }
}