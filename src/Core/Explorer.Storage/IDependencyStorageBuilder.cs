using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public interface IDependencyStorageBuilder
    {
        IServiceCollection Services { get; }
    }
}