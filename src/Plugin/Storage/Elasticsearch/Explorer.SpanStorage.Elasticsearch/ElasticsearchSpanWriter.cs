using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.Storage.Abstractions;
using Explorer.Storage.Elasticsearch;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Explorer.SpanStorage.Elasticsearch
{
    public class ElasticsearchSpanWriter : ISpanWriter
    {
        private readonly ElasticsearchOptions _options;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IIndexManager _indexManager;

        public ElasticsearchSpanWriter(
            IOptions<ElasticsearchOptions> optionsAccessor,
            ILogger<ElasticsearchSpanWriter> logger,
            IHttpClientFactory httpClientFactory,
            IIndexManager indexManager)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _indexManager = indexManager;
            _options = optionsAccessor.Value;
        }

        public async Task WriteAsync(IEnumerable<Span> spans)
        {
            await _indexManager.CreateIndexIfAbsentAsync(ElasticsearchStorageConstants.SpanIndexName, () =>
            {
                var fileProvider = new EmbeddedFileProvider(Assembly.GetExecutingAssembly());
                using var readStream = fileProvider
                    .GetFileInfo($"Mappings.{ElasticsearchStorageConstants.SpanIndexName}.json").CreateReadStream();
                using var streamReader = new StreamReader(readStream);
                return streamReader.ReadToEnd();
            });

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

            var action = $"{{\"index\":{{\"_index\":\"{ElasticsearchStorageConstants.SpanIndexName}\"}}}}";
            var request = string.Join(
                Environment.NewLine,
                data.Select(item => $"{action}{Environment.NewLine}{item.ToJson()}")) + Environment.NewLine;

            await httpClient.PostAsync($"{_options.URL}/_bulk", new StringContent(request, Encoding.UTF8,
                "application/json"));
        }
    }
}