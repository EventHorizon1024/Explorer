using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.Storage.Abstractions;

namespace Explorer.Storage
{
    public class InMemorySpanWriter: ISpanWriter
    {
        public Task WriteAsync(IEnumerable<Span> spans)
        {
            throw new System.NotImplementedException();
        }
    }
}