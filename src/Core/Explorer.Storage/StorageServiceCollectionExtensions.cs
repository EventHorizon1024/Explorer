using System;
using Explorer.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public static class StorageServiceCollectionExtensions
    {
        public static IServiceCollection AddExplorerSpanStorage(
            this IServiceCollection services,
            Action<ISpanStorageBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ISpanProcessor, SpanProcessor>();

            configure(new SpanStorageBuilder(services));

            return services;
        }
        
        public static IServiceCollection AddExplorerDependencyStorage(
            this IServiceCollection services,
            Action<IDependencyStorageBuilder> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            configure(new DependencyStorageBuilder(services));

            return services;
        }
    }
}