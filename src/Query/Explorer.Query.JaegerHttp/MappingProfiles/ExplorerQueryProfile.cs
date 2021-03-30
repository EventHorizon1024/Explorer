using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Explorer.Query.JaegerHttp.DTOs.Request;
using Explorer.Models;
using Explorer.Query.JaegerHttp.DTOs.Response;
using Explorer.Storage.Abstractions;

namespace Explorer.Query.JaegerHttp.MappingProfiles
{
    public class ExplorerQueryProfile : Profile
    {
        public ExplorerQueryProfile()
        {
            CreateMap<SpanReference, SpanReferenceDTO>()
                .ForMember(dest => dest.RefType,
                    opt => opt.MapFrom(
                        src => ConvertRefType(src.RefType)));

            CreateMap<Span, SpanDTO>()
                .ForMember(dest => dest.References,
                    opt => opt.MapFrom(src => src.References));

            CreateMap<Trace, TraceDTO>()
                .ForMember(dest => dest.Spans,
                    opt => opt.MapFrom(src => src.Spans))
                .AfterMap(ExtractProcessFromSpans);

            CreateMap<Dependency, DependencyDTO>();
        }

        private static string ConvertRefType(SpanRefType refType) =>
            refType switch
            {
                SpanRefType.ChildOf => "CHILD_OF",
                SpanRefType.FollowsFrom => "FOLLOWS_FROM"
            };

        private void ExtractProcessFromSpans(Trace trace, TraceDTO traceDTO)
        {
            traceDTO.Processes = new Dictionary<string, ProcessDTO>();
            var processes = traceDTO.Processes;

            for (int i = 0; i < traceDTO.Spans.Length; i++)
            {
                var span = traceDTO.Spans[i];
                var process = trace.Spans[i].Process;
                var pid = $"p{i + 1}";
                span.ProcessID = pid;

                processes[pid] = new ProcessDTO
                {
                    ProcessID = pid,
                    ServiceName = process.ServiceName,
                    Tags = process.Tags
                };
            }
        }
    }
}