using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        
        Memory<byte> ViewModelJson = Array.Empty<byte>();

        public void ViewModelSerialized(IDotvvmRequestContext context, int viewModelSize, Func<Stream> viewModelBuffer)
        {
            if (informationSender.State >= DiagnosticsInformationSenderState.Full)
            {
                using (var stream = viewModelBuffer())
                {
                    ViewModelJson = stream.ReadToMemory();
                }
            }
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
                request.RequestType,
                request.HttpContext.Request.Method,
                request.HttpContext.Request.Url.AbsolutePath,
                request.HttpContext.Request.Headers.Select(HttpHeaderItem.FromKeyValuePair),
                request.ReceivedViewModelJson?.RootElement.GetPropertyOrNull("viewModel")?.GetRawText()
            );
        }

        private ResponseDiagnostics BuildResponseDiagnostics(IDotvvmRequestContext request)
        {
            return new ResponseDiagnostics
            {
                StatusCode = request.HttpContext.Response.StatusCode,
                Headers = request.HttpContext.Response.Headers.Select(HttpHeaderItem.FromKeyValuePair)
                    .ToList(),
                ViewModelJson = StringUtils.Utf8Decode(this.ViewModelJson.Span),
                // ViewModelDiff = request.ViewModelJson?.GetValue("viewModelDiff")?.ToString(), // TODO: how do we have diffs now?
                ResponseSize = ResponseSize?.realLength ?? -1,
                CompressedResponseSize = ResponseSize?.compressedLength ?? -1
            };
        }
    }

}
