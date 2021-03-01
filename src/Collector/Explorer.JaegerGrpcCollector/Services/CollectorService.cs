using System.Collections.Generic;
using System.Threading.Tasks;
using Explorer.Storage.Abstractions;
using Grpc.Core;
using Jaeger.ApiV2;
using Span = Explorer.Models.Span;

namespace Explorer.JaegerGrpcCollector.Services
{
    public class JaegerCollectorService : CollectorService.CollectorServiceBase
    {
        private readonly ISpanProcessor _processor;

        public JaegerCollectorService(ISpanProcessor processor)
        {
            _processor = processor;
        }

        public override async Task<PostSpansResponse> PostSpans(PostSpansRequest request, ServerCallContext context)
        {
            var spans = ExplorerSpanConverter.ConvertSpanFromJagger(request);
            await _processor.ProcessAsync(spans);
            return new PostSpansResponse();
        }
    }
}