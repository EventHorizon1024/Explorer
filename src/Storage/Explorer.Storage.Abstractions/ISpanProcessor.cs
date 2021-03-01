using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Storage.Abstractions
{
    public interface ISpanProcessor
    {
        Task ProcessAsync(IEnumerable<Span> spans);
    }
}