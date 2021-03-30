using System;
using Explorer.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public static class StorageServiceCollectionExtensions
    {
        public static IServiceCollection AddExplorerSpanStorage(
            this IServiceCollection services,
            Action<IStorageBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ISpanProcessor, SpanProcessor>();

            configure(new StorageBuilder(services));

            return services;
        }
        
        public static IServiceCollection AddExplorerDependencyStorage(
            this IServiceCollection services,
            Action<IStorageBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            configure(new StorageBuilder(services));

            return services;
        }
    }
}