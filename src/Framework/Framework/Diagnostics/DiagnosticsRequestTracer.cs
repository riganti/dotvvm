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
            return new EventTiming(
                eventName,
                stopwatch.ElapsedMilliseconds - ElapsedMillisSinceLastLog,
                stopwatch.ElapsedMilliseconds
            );
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
            return new DiagnosticsInformation(
                BuildRequestDiagnostics(request),
                BuildResponseDiagnostics(request),
                events,
                stopwatch.ElapsedMilliseconds
            );
        }

        private RequestDiagnostics BuildRequestDiagnostics(IDotvvmRequestContext request)
        {
            return new RequestDiagnostics(
                RequestTypeFromContext(request),
                request.HttpContext.Request.Method,
                request.HttpContext.Request.Url.AbsolutePath,
                request.HttpContext.Request.Headers.Select(HttpHeaderItem.FromKeyValuePair),
                request.ReceivedViewModelJson?.GetValue("viewModel")?.ToString()
            );
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
