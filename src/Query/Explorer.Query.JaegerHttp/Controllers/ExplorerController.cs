using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Explorer.Models;
using Explorer.Query.JaegerHttp.DTOs;
using Explorer.Query.JaegerHttp.DTOs.Request;
using Explorer.Query.JaegerHttp.DTOs.Response;
using Explorer.Storage.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Explorer.Query.JaegerHttp.Controllers
{
    [Route("/api")]
    public class ExplorerController : Controller
    {
        private readonly ISpanReader _spanReader;
        private readonly IMapper _mapper;

        public ExplorerController(ISpanReader spanReader, IMapper mapper)
        {
            _spanReader = spanReader;
            _mapper = mapper;
        }

        [HttpGet("services")]
        public async Task<JaegerResult<string[]>> GetSeries()
        {
            return new(await _spanReader.GetSeriesAsync());
        }

        [HttpGet("services/{serviceName}/operations")]
        public async Task<JaegerResult<string[]>> GetOperations(string serviceName)
        {
            return new(await _spanReader.GetOperationsAsync(serviceName));
        }

        [HttpGet("traces")]
        public async Task<JaegerResult<TraceDTO[]>> FindTraces([FromQuery] FindTracesReqDto request)
        {
            static long? ParseAsMicroseconds(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }

                var m = Regex.Match(input,
                    @"^((?<days>\d+)d)?((?<hours>\d+)h)?((?<minutes>\d+)m)?((?<seconds>\d+)s)?((?<milliseconds>\d+)ms)?((?<microseconds>\d+)μs)?$",
                    RegexOptions.ExplicitCapture
                    | RegexOptions.Compiled
                    | RegexOptions.CultureInvariant
                    | RegexOptions.RightToLeft);

                long days = m.Groups["days"].Success ? long.Parse(m.Groups["days"].Value) : 0;
                long hours = m.Groups["hours"].Success ? long.Parse(m.Groups["hours"].Value) : 0;
                long minutes = m.Groups["minutes"].Success ? long.Parse(m.Groups["minutes"].Value) : 0;
                long seconds = m.Groups["seconds"].Success ? long.Parse(m.Groups["seconds"].Value) : 0;
                long milliseconds = m.Groups["milliseconds"].Success ? long.Parse(m.Groups["milliseconds"].Value) : 0;
                long microseconds = m.Groups["microseconds"].Success ? long.Parse(m.Groups["microseconds"].Value) : 0;

                return
                    ((days * 24 * 60 * 60 + hours * 60 * 60 + minutes * 60 + seconds) * 1000 + milliseconds) * 1000 +
                    microseconds;
            }


            long? startTimeMin = request.Start;
            long? startTimeMax = request.End;

            var lookBack = ParseAsMicroseconds(request.LookBack);

            if (lookBack.HasValue)
            {
                var now = DateTimeOffset.Now.ToUnixTimeMicroseconds();
                startTimeMin = now - lookBack.Value;
                startTimeMax = now;
            }

            Trace[] traces;

            if (request.TraceID?.Any() ?? false)
            {
                traces = await _spanReader.FindTracesAsync(request.TraceID, startTimeMin, startTimeMax);
            }
            else
            {
                traces = await _spanReader.FindTracesAsync(new TraceQueryParameters
                {
                    ServiceName = request.Service,
                    OperationName = request.Operation,
                    Tags = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.Tags ?? "{}"),
                    StartTimeMin = startTimeMin,
                    StartTimeMax = startTimeMax,
                    DurationMin = ParseAsMicroseconds(request.MinDuration),
                    DurationMax = ParseAsMicroseconds(request.MaxDuration),
                    NumTraces = request.Limit
                });
            }

            JaegerResultError error = null;
            if (traces.Length == 0)
            {
                error = new JaegerResultError
                {
                    Code = (int) HttpStatusCode.NotFound,
                    Message = "trace not found"
                };
            }

            return new JaegerResult<TraceDTO[]>(_mapper.Map<TraceDTO[]>(traces))
            {
                Error = error
            };
        }

        [HttpGet("traces/{traceID}")]
        public async Task<JaegerResult<TraceDTO[]>> GetTrace(string traceID)
        {
            var traces = await _spanReader.FindTracesAsync(new string[] {traceID});

            JaegerResultError error = null;
            if (traces.Length == 0)
            {
                error = new JaegerResultError
                {
                    Code = (int) HttpStatusCode.NotFound,
                    Message = "trace not found"
                };
            }

            return new JaegerResult<TraceDTO[]>(_mapper.Map<TraceDTO[]>(traces))
            {
                Error = error
            };
        }
    }
}