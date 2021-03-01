using Explorer.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Explorer.Storage
{
    public static class StorageBuilderExtensions
    {
        public static IStorageBuilder UseInMemoryDB(this IStorageBuilder builder)
        {
            builder.Services.AddSingleton<ISpanWriter, InMemorySpanWriter>();
            return builder;
        }
    }
}