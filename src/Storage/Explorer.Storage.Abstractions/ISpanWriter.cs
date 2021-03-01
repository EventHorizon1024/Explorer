using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Models;

namespace Explorer.Storage.Abstractions
{
    public interface ISpanWriter
    {
        Task WriteAsync(IEnumerable<Span> spans);
    }
}