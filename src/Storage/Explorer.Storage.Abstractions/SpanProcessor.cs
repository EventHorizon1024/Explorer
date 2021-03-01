using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Explorer.Models;
using Microsoft.Extensions.Logging;

namespace Explorer.Storage.Abstractions
{
    public class SpanProcessor : ISpanProcessor
    {
        private readonly ChannelWriter<IEnumerable<Span>> _channelWriter;

        private readonly ISpanWriter _spanStorage;
        private readonly ChannelReader<IEnumerable<Span>> _channelReader;
        private readonly ILogger<SpanProcessor> _logger;

        public SpanProcessor(ISpanWriter spanStorage, ILogger<SpanProcessor> logger)
        {
            _spanStorage = spanStorage;
            _logger = logger;
            var channel = Channel.CreateUnbounded<IEnumerable<Span>>();
            _channelWriter = channel.Writer;
            _channelReader = channel.Reader;

            var taskFactory = new TaskFactory();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                taskFactory.StartNew(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            await ConsumeAsync();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to consume span");
                        }
                    }
                }, TaskCreationOptions.LongRunning);
            }
        }

        public Task ProcessAsync(IEnumerable<Span> spans)
        {
            _channelWriter.WriteAsync(spans);
            return Task.CompletedTask;
        }

        private async Task ConsumeAsync()
        {
            var spans = await _channelReader.ReadAsync();

            await _spanStorage.WriteAsync(spans);
        }
    }
}