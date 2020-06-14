using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Diagnostics
{

    public class DiagnosticsRequestTracer : IRequestTracer
    {
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly IDiagnosticsInformationSender informationSender;
        private IList<EventTiming> events = new List<EventTiming>();

        private long ElapsedMillisSinceLastLog => events.Sum(e => e.Duration);

        private (long compressedLength, long realLength)? ResponseSize;

        public DiagnosticsRequestTracer(IDiagnosticsInformationSender sender)
        {
            this.informationSender = sender;
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            if (eventName == RequestTracingConstants.BeginRequest)
            {
                stopwatch.Start();
            }
            events.Add(CreateEventTiming(eventName));
            return TaskUtils.GetCompletedTask();
        }

        private EventTiming CreateEventTiming(string eventName)
        {
            return new EventTiming
            {
                Duration = stopwatch.ElapsedMilliseconds - ElapsedMillisSinceLastLog,
                TotalDuration = stopwatch.ElapsedMilliseconds,
                EventName = eventName
            };
        }

        public Task EndRequest(IDotvvmRequestContext context)
        {
            stopwatch.Stop();
            var diagnosticsData = BuildDiagnosticsData(context);
            Reset();
            return informationSender.SendInformationAsync(diagnosticsData);
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            stopwatch.Stop();
            var diagnosticsData = BuildDiagnosticsData(context);
            diagnosticsData.ResponseDiagnostics.StatusCode = 500;
            diagnosticsData.ResponseDiagnostics.ExceptionStackTrace = exception.ToString();
            Reset();
            return informationSender.SendInformationAsync(diagnosticsData);
        }

        public void LogResponseSize(long compressedSize, long realSize)
        {
            this.ResponseSize = (compressedSize, realSize);
        }

        private void Reset()
        {
            stopwatch.Reset();
            events = new List<EventTiming>();
        }

        private DiagnosticsInformation BuildDiagnosticsData(IDotvvmRequestContext request)
        {
            return new DiagnosticsInformation
            {
                RequestDiagnostics = BuildRequestDiagnostics(request),
                ResponseDiagnostics = BuildResponseDiagnostics(request),
                EventTimings = events,
                TotalDuration = stopwatch.ElapsedMilliseconds
            };
        }

        private RequestDiagnostics BuildRequestDiagnostics(IDotvvmRequestContext request)
        {
            return new RequestDiagnostics
            {
                RequestType = RequestTypeFromContext(request),
                Method = request.HttpContext.Request.Method,
                Url = request.HttpContext.Request.Url.AbsolutePath,
                Headers = request.HttpContext.Request.Headers.Select(HttpHeaderItem.FromKeyValuePair)
                    .ToList(),
                ViewModelJson = request.ReceivedViewModelJson?.GetValue("viewModel")?.ToString()
            };
        }

        private RequestType RequestTypeFromContext(IDotvvmRequestContext context)
        {
            if (context.ReceivedViewModelJson == null && context.ViewModelJson != null)
            {
                return RequestType.Get;
            }
            else if (context.ReceivedViewModelJson != null)
            {
                return RequestType.Command;
            }
            else
            {
                return RequestType.StaticCommand;
            }
        }

        private ResponseDiagnostics BuildResponseDiagnostics(IDotvvmRequestContext request)
        {
            return new ResponseDiagnostics
            {
                StatusCode = request.HttpContext.Response.StatusCode,
                Headers = request.HttpContext.Response.Headers.Select(HttpHeaderItem.FromKeyValuePair)
                    .ToList(),
                ViewModelJson = request.ViewModelJson?.GetValue("viewModel")?.ToString(),
                ViewModelDiff = request.ViewModelJson?.GetValue("viewModelDiff")?.ToString(),
                ResponseSize = ResponseSize?.realLength ?? -1,
                CompressedResponseSize = ResponseSize?.compressedLength ?? -1
            };
        }
    }

}