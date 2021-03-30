using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Storage.Abstractions
{
    public interface IDependencyReader
    {
        Task<Dependency[]> GetDependenciesAsync(long startTime, long endTime);
    }
}