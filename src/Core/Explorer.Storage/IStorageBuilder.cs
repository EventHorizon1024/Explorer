using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }
    }
}