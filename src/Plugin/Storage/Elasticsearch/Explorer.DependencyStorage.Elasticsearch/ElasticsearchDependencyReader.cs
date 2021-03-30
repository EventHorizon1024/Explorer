using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.SpanStorage.Elasticsearch;
using Explorer.SpanStorage.Elasticsearch.Models;
using Explorer.Storage.Abstractions;
using Explorer.Storage.Elasticsearch;
using Microsoft.Extensions.Options;

namespace Explorer.DependencyStorage.Elasticsearch
{
    public class ElasticsearchDependencyReader : IDependencyReader
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ElasticsearchOptions _options;

        public ElasticsearchDependencyReader(
            IHttpClientFactory httpClientFactory,
            IOptions<ElasticsearchOptions> optionsAccessor
        )
        {
            _httpClientFactory = httpClientFactory;
            _options = optionsAccessor.Value;
        }

        public async Task<Dependency[]> GetDependenciesAsync(long startTime, long endTime)
        {
            var client = _httpClientFactory.CreateClient();

            var request =
                $"{{\"size\":10000,\"query\":{{\"bool\":{{\"must\":[{{\"range\":{{\"startTime\":{{\"gte\":{startTime},\"lte\":{endTime}}}}}}}]}}}}}}";

            var httpResponseMessage = await client.PostAsync(
                $"{_options.URL}/{ElasticsearchStorageConstants.SpanIndexName}/_search",
                new StringContent(request, Encoding.UTF8, "application/json"));
            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return Array.Empty<Dependency>();
            }
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            var result = response.FromJson<SearchResponse<Span>>();
            var spans = result.Data;

            var dependencies = spans.Where(span => span.References.Any(r => r.RefType == SpanRefType.ChildOf))
                .GroupBy
                (
                    childSpan =>
                    {
                        var @ref = childSpan.References.First();
                        var parent = spans.FirstOrDefault(s => s.SpanID == @ref.SpanID);
                        return new
                        {
                            Parent = parent?.Process?.ServiceName,
                            Child = childSpan.Process?.ServiceName
                        };
                    })
                .Where(g => g.Key.Parent != g.Key.Child).Select(g => new Dependency
                {
                    Parent = g.Key.Parent,
                    Child = g.Key.Child,
                    CallCount = g.Count()
                }).ToArray();

            return dependencies;
        }
    }
}