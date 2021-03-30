using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Explorer.SpanStorage.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Explorer.Storage.Elasticsearch
{
    public class IndexManager : IIndexManager
    {
        private readonly IHttpClientFactory _factory;
        private readonly ElasticsearchOptions _options;
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _locker;
        private volatile bool _indexExisted;

        public IndexManager(
            IOptions<ElasticsearchOptions> optionsAccessor,
            IHttpClientFactory factory,
            ILogger<IndexManager> logger)
        {
            _factory = factory;
            _options = optionsAccessor.Value;
            _logger = logger;
            _locker = new SemaphoreSlim(1);
        }

        public async ValueTask CreateIndexIfAbsentAsync(string indexName, Func<string> mappingProvider)
        {
            if (_indexExisted) return;

            try
            {
                await _locker.WaitAsync();

                var client = _factory.CreateClient();
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head,
                    $"{_options.URL}/{indexName}"));

                _indexExisted = response.StatusCode == HttpStatusCode.OK;
                if (_indexExisted) return;

                _indexExisted = await TryCreateIndexAsync(indexName, mappingProvider);
            }
            finally
            {
                _locker.Release();
            }
        }

        private async Task<bool> TryCreateIndexAsync(string indexName, Func<string> mappingProvider)
        {
            try
            {
                var client = _factory.CreateClient();

                var spanMapping = mappingProvider();

                var response = await client.PutAsync($"{_options.URL}/{indexName}",
                    new StringContent(spanMapping, Encoding.UTF8, "application/json"));

                response.EnsureSuccessStatusCode();
                _logger.LogInformation($"Create index {indexName} successfully");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to create index {indexName}");
                throw;
            }
        }
    }
}