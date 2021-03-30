using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public class DependencyStorageBuilder:IDependencyStorageBuilder
    {
        public DependencyStorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}