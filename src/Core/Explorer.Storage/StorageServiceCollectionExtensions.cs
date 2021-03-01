using System;
using Explorer.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public static class StorageServiceCollectionExtensions
    {
        public static IServiceCollection AddExplorerStorage(this IServiceCollection services)
        {
            return AddExplorerStorage(services, builder => builder.UseInMemoryDB());
        }

        public static IServiceCollection AddExplorerStorage(this IServiceCollection services, Action<IStorageBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ISpanProcessor, SpanProcessor>();
            
            configure(new StorageBuilder(services));

            return services;
        }
    }
}