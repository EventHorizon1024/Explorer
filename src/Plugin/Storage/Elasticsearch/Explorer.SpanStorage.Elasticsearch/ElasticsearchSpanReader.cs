using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Explorer.Models;
using Explorer.SpanStorage.Elasticsearch.Models;
using Explorer.Storage.Abstractions;
using Explorer.Storage.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Explorer.SpanStorage.Elasticsearch
{
    public class ElasticsearchSpanReader : ISpanReader
    {
        private readonly ElasticsearchOptions _options;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ElasticsearchSpanReader(
            IOptions<ElasticsearchOptions> optionsAccessor,
            ILogger<ElasticsearchSpanReader> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _options = optionsAccessor.Value;
        }

        public async Task<string[]> GetSeriesAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var query =
                "{\"size\":0,\"aggs\":{\"serviceName\":{\"terms\":{\"size\":10000,\"field\":\"process.serviceName.keyword\"}}}}";
            var httpResponseMessage = await client.PostAsync($"{_options.URL}/{ElasticsearchStorageConstants.SpanIndexName}/_search",
                new StringContent(query, Encoding.UTF8, "application/json"));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            return response.FromJson<SearchResponse<object>>().Aggregations["serviceName"].Keys.ToArray();
        }

        public async Task<string[]> GetOperationsAsync(string serviceName)
        {
            var client = _httpClientFactory.CreateClient();
            var query =
                "{\"size\":0,\"query\":{\"bool\":{\"must\":[{\"match\":{\"process.serviceName\":\"" + serviceName +
                "\"}}]}}" +
                ",\"aggs\":{\"operationName\":{\"terms\":{\"size\":10000,\"field\":\"operationName.keyword\"}}}}";
            var httpResponseMessage = await client.PostAsync($"{_options.URL}/{ElasticsearchStorageConstants.SpanIndexName}/_search",
                new StringContent(query, Encoding.UTF8, "application/json"));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            return response.FromJson<SearchResponse<object>>().Aggregations["operationName"].Keys.ToArray();
        }

        public async Task<Trace[]> FindTracesAsync(TraceQueryParameters query)
        {
            var traceIDs = await FindTraceIDsAsync(query);
            if (traceIDs.Length == 0)
            {
                return Array.Empty<Trace>();
            }

            return await FindTracesAsync(traceIDs, query.StartTimeMin, query.StartTimeMax);
        }

        public async Task<Trace[]> FindTracesAsync(string[] traceIDs, long? startTime = null, long? endTime = null)
        {
            var index = $"{{\"index\":\"{ElasticsearchStorageConstants.SpanIndexName}\"}}";
            string startTimeCondition = null;
            if (startTime.HasValue || endTime.HasValue)
            {
                startTimeCondition += ",{\"range\":{\"startTime\":";
                if (startTime.HasValue)
                {
                    startTimeCondition += $"{{\"gte\":{startTime}";
                }

                if (endTime.HasValue)
                {
                    startTimeCondition += $",\"lte\":{endTime}";
                }

                startTimeCondition += "}}}";
            }

            var stringBuilder = new StringBuilder();
            foreach (var traceID in traceIDs)
            {
                stringBuilder.AppendLine(index);
                stringBuilder.Append(
                    "{\"size\":10000,\"query\":{\"bool\":{\"must\":[{\"match\":{\"traceID\":\"" + traceID + "\"}}");
                stringBuilder.Append(startTimeCondition);
                stringBuilder.AppendLine("]}}}");
            }

            var request = stringBuilder.ToString();
            var client = _httpClientFactory.CreateClient();
            var httpResponseMessage = await client.PostAsync($"{_options.URL}/_msearch",
                new StringContent(request, Encoding.UTF8, "application/json"));
            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            var result = response.FromJson<MultiSearchResponse<Span>>();

            var spans = result.Responses.SelectMany(r => r.Data).ToLookup(s => s.TraceID);

            var traces = spans.Select(s => new Trace
            {
                TraceID = s.Key,
                Spans = s.ToArray()
            }).ToArray();
            return traces;
        }

        private async Task<string[]> FindTraceIDsAsync(TraceQueryParameters query)
        {
            string BuildQueryCondition()
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append("          { \"match\": { \"process.serviceName\": \"" + query.ServiceName +
                                     "\" }}");
                if (!string.IsNullOrWhiteSpace(query.OperationName))
                {
                    stringBuilder.AppendLine(",");
                    stringBuilder.Append("          { \"match\": { \"operationName\":   \"" + query.OperationName +
                                         "\"}}");
                }

                if (query.StartTimeMin.HasValue || query.StartTimeMax.HasValue)
                {
                    stringBuilder.AppendLine(",");
                    stringBuilder.AppendLine("          {");
                    stringBuilder.AppendLine("            \"range\": {");
                    stringBuilder.AppendLine("              \"startTime\": {");
                    if (query.StartTimeMin.HasValue)
                    {
                        stringBuilder.AppendLine("                \"gte\": " + query.StartTimeMin);
                    }

                    if (query.StartTimeMax.HasValue)
                    {
                        stringBuilder.AppendLine("                ,\"lte\": " + query.StartTimeMax);
                    }

                    stringBuilder.AppendLine("              }");
                    stringBuilder.AppendLine("            }");
                    stringBuilder.Append("          }");
                }

                if (query.DurationMin.HasValue || query.DurationMax.HasValue)
                {
                    stringBuilder.AppendLine(",");
                    stringBuilder.AppendLine("          {");
                    stringBuilder.AppendLine("            \"range\": {");
                    stringBuilder.AppendLine("              \"duration\": {");
                    if (query.DurationMin.HasValue)
                    {
                        stringBuilder.AppendLine("                \"gte\": " + query.DurationMin);
                    }

                    if (query.DurationMax.HasValue)
                    {
                        stringBuilder.AppendLine("                ,\"lte\": " + query.DurationMax);
                    }

                    stringBuilder.AppendLine("              }");
                    stringBuilder.AppendLine("            }");
                    stringBuilder.Append("          }");
                }

                if (query.Tags?.Any() ?? false)
                {
                    foreach (var (key, value) in query.Tags)
                    {
                        stringBuilder.AppendLine(",");
                        stringBuilder.AppendLine("        {");
                        stringBuilder.AppendLine("\"bool\": {");
                        stringBuilder.AppendLine("              \"should\": [");
                        stringBuilder.AppendLine("                   { \"nested\" : {");
                        stringBuilder.AppendLine("                      \"path\" : \"tags\",");
                        stringBuilder.AppendLine("                      \"query\" : {");
                        stringBuilder.AppendLine("                          \"bool\" : {");
                        stringBuilder.AppendLine("                              \"must\" : [");
                        stringBuilder.AppendLine("                              { \"match\" : {\"tags.key\" : \"" +
                                                 key + "\"} },");
                        stringBuilder.AppendLine("                              { \"match\" : {\"tags.value\" : \"" +
                                                 value + "\"} }");
                        stringBuilder.AppendLine("                              ]");
                        stringBuilder.AppendLine("                          }}}},");
                        stringBuilder.AppendLine("                   { \"nested\" : {");
                        stringBuilder.AppendLine("                          \"path\" : \"process.tags\",");
                        stringBuilder.AppendLine("                          \"query\" : {");
                        stringBuilder.AppendLine("                              \"bool\" : {");
                        stringBuilder.AppendLine("                                  \"must\" : [");
                        stringBuilder.AppendLine("                                  { \"match\" : {\"tags.key\" : \"" +
                                                 key + "\"} },");
                        stringBuilder.AppendLine(
                            "                                  { \"match\" : {\"tags.value\" : \"" + value + "\"} }");
                        stringBuilder.AppendLine("                                  ]");
                        stringBuilder.AppendLine("                              }}}},");
                        stringBuilder.AppendLine("                   { \"nested\" : {");
                        stringBuilder.AppendLine("                          \"path\" : \"logs.fields\",");
                        stringBuilder.AppendLine("                          \"query\" : {");
                        stringBuilder.AppendLine("                              \"bool\" : {");
                        stringBuilder.AppendLine("                                  \"must\" : [");
                        stringBuilder.AppendLine("                                  { \"match\" : {\"tags.key\" : \"" +
                                                 key + "\"} },");
                        stringBuilder.AppendLine(
                            "                                  { \"match\" : {\"tags.value\" : \"" + value + "\"} }");
                        stringBuilder.AppendLine("                                  ]");
                        stringBuilder.Append("                              }}}}");
                    }

                    stringBuilder.AppendLine("                ]");
                    stringBuilder.AppendLine("              }");
                    stringBuilder.Append("            }");
                }

                return stringBuilder.ToString();
            }

            var sbBuilder = new StringBuilder();

            sbBuilder.AppendLine("{");
            sbBuilder.AppendLine("    \"size\": 0,");
            sbBuilder.AppendLine("    \"query\": {");
            sbBuilder.AppendLine("      \"bool\": {");
            sbBuilder.AppendLine("        \"must\": [");
            sbBuilder.AppendLine(BuildQueryCondition());
            sbBuilder.AppendLine("        ]");
            sbBuilder.AppendLine("      }");
            sbBuilder.AppendLine("    },");
            sbBuilder.AppendLine(    "\"sort\": [{ \"startTime\": { \"order\": \"desc\" }}],");
            sbBuilder.AppendLine(
                "    \"aggs\": { \"traceIDs\": { \"terms\" : {\"size\": "
                + query.NumTraces +
                ",\"field\": \"traceID.keyword\" }}}");
            sbBuilder.AppendLine("}");

            var client = _httpClientFactory.CreateClient();
            
            var request = sbBuilder.ToString();
            var httpResponseMessage = await client.PostAsync($"{_options.URL}/{ElasticsearchStorageConstants.SpanIndexName}/_search",
                new StringContent(request, Encoding.UTF8, "application/json"));

            var response = await httpResponseMessage.Content.ReadAsStringAsync();
            var result = response.FromJson<SearchResponse<object>>();

            return result.Aggregations["traceIDs"].Buckets.Select(b => b.Key).ToArray();
        }
    }
}