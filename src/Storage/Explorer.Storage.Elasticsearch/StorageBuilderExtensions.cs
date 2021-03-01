using System;
using Explorer.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage.Elasticsearch
{
    public static class StorageBuilderExtensions
    {
        public static IStorageBuilder UseElasticsearch(this IStorageBuilder builder, Action<ElasticsearchOptions> configureOptions)
        {
            builder.Services.PostConfigure(configureOptions);
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ISpanWriter, ElasticsearchSpanWriter>();
            builder.Services.AddSingleton<ISpanReader, ElasticsearchSpanReader>();
            return builder;
        }
    }
}