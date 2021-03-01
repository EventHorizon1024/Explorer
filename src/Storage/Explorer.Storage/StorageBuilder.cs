using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    internal class StorageBuilder : IStorageBuilder
    {
        public StorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}