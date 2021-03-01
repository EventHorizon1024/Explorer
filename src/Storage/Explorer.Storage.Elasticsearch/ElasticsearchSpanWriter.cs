using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.Storage.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Explorer.Storage.Elasticsearch
{
    public class ElasticsearchSpanWriter : ISpanWriter
    {
        private readonly ElasticsearchOptions _options;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ElasticsearchSpanWriter(
            IOptions<ElasticsearchOptions> optionsAccessor,
            ILogger<ElasticsearchSpanWriter> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _options = optionsAccessor.Value;
        }

        public async Task WriteAsync(IEnumerable<Span> spans)
        {
            try
            {
                await BulkInsertAsync(spans);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save spans to Elasticsearch");
            }
        }

        private async Task BulkInsertAsync<T>(IEnumerable<T> data)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var action = $"{{\"index\":{{\"_index\":\"{_options.IndexName}\"}}}}";
            var request = string.Join(
                Environment.NewLine,
                data.Select(item => $"{action}{Environment.NewLine}{item.ToJson()}")) + Environment.NewLine;

            await httpClient.PostAsync($"{_options.Url}/_bulk", new StringContent(request, Encoding.UTF8,
                "application/json"));
        }
    }
}