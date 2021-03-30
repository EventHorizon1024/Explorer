using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public interface ISpanStorageBuilder
    {
        IServiceCollection Services { get; }
    }
}