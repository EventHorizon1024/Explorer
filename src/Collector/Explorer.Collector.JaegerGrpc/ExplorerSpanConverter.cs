using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Explorer.Models;
using Explorer.Storage.Abstractions;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Jaeger.ApiV2;
using Span = Explorer.Models.Span;
using SpanRefType = Explorer.Models.SpanRefType;
using Process = Explorer.Models.Process;
using ValueType = Jaeger.ApiV2.ValueType;

namespace Explorer.Collector.JaegerGrpc
{
    public static class ExplorerSpanConverter
    {
        public static IEnumerable<Span> ConvertSpanFromJagger(PostSpansRequest jaegerSpanRequest)
        {
            var batch = jaegerSpanRequest.Batch;
            return batch.Spans.Select(span =>
            {
                var process = span.Process ?? batch.Process;
                return new Span
                {
                    TraceID = ConvertByteStringToTracingId(span.TraceId),
                    SpanID = ConvertByteStringToSpanId(span.SpanId),
                    OperationName = span.OperationName,
                    Flags = span.Flags,
                    StartTime = span.StartTime.ToDateTimeOffset().ToUnixTimeMicroseconds(),
                    Duration = (long) span.Duration.Nanos / 1000,
                    Process = new Process
                    {
                        ServiceName = process.ServiceName,
                        Tags = ConvertKeyValuesToTags(process.Tags)
                    },
                    References = span.References.Select(@ref => new SpanReference
                    {
                        TraceID = ConvertByteStringToTracingId(@ref.TraceId),
                        SpanID = ConvertByteStringToSpanId(@ref.SpanId),
                        RefType = @ref.RefType switch
                        {
                            Jaeger.ApiV2.SpanRefType.ChildOf => SpanRefType.ChildOf,
                            Jaeger.ApiV2.SpanRefType.FollowsFrom => SpanRefType.FollowsFrom,
                            _ => throw new ArgumentOutOfRangeException()
                        }
                    }).ToArray(),
                    Tags = ConvertKeyValuesToTags(span.Tags),
                    Logs = span.Logs.Select(log => new SpanLog
                    {
                        Timestamp = log.Timestamp.ToDateTimeOffset(),
                        Fields = ConvertKeyValuesToTags(log.Fields)
                    }).ToArray(),

                    Warnings = span.Warnings.ToArray()
                };
            }).ToArray();
        }

        private static string ConvertByteStringToSpanId(ByteString byteString) =>
            ConvertByteStringToLong(byteString.Span).ToString("x016");

        private static string ConvertByteStringToTracingId(ByteString byteString)
        {
            var high = ConvertByteStringToLong(byteString.Span[..8]);
            var low = ConvertByteStringToLong(byteString.Span[8..16]);
            return high == 0 ? low.ToString("x016") : $"{high:x016}{low:x016}";
        }

        private static long ConvertByteStringToLong(ReadOnlySpan<byte> bytes) =>
            BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReadInt64BigEndian(bytes)
                : BinaryPrimitives.ReadInt64LittleEndian(bytes);

        private static Tag[] ConvertKeyValuesToTags(RepeatedField<KeyValue> fields)
        {
            var kvs = new Tag[fields.Count];

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                object value = field.VType switch
                {
                    ValueType.String => field.VStr,
                    ValueType.Bool => field.VBool,
                    ValueType.Int64 => field.VInt64,
                    ValueType.Float64 => field.VFloat64,
                    ValueType.Binary => field.VBinary,
                    _ => throw new ArgumentOutOfRangeException()
                };
                kvs[i] = new Tag
                {
                    Key = field.Key,
                    Type = field.VType.ToString().ToLowerInvariant(),
                    Value = value
                };
            }

            return kvs;
        }
    }
}