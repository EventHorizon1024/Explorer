using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    internal class SpanStorageBuilder : ISpanStorageBuilder
    {
        public SpanStorageBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}