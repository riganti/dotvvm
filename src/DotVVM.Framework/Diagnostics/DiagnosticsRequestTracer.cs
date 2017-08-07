using System;
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

            return TaskUtils.GetCompletedTask();
        }

        public Task EndRequest(IDotvvmRequestContext context)
        {
            stopwatch.Stop();
            var diagnosticsData = BuildDiagnosticsData(context);
            return informationSender.SendInformationAsync(diagnosticsData);
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            stopwatch.Stop();
            var diagnosticsData = BuildDiagnosticsData(context);
            diagnosticsData.ResponseDiagnostics.StatusCode = 500;
            diagnosticsData.ResponseDiagnostics.ExceptionStackTrace = exception.ToString();
            return informationSender.SendInformationAsync(diagnosticsData);
        }

        private DiagnosticsInformation BuildDiagnosticsData(IDotvvmRequestContext request)
        {
            return new DiagnosticsInformation
            {
                RequestDiagnostics = BuildRequestDiagnostics(request),
                ResponseDiagnostics = BuildResponseDiagnostics(request),
                TotalDuration = stopwatch.ElapsedMilliseconds
            };
        }

        private RequestDiagnostics BuildRequestDiagnostics(IDotvvmRequestContext request)
        {
            return new RequestDiagnostics
            {
                RequestType = RequestTypeFromContext(request),
                Method = request.HttpContext.Request.Method,
                Url = request.HttpContext.Request.Path.Value,
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
                ResponseSize = GetResponseContentLength(request)
            };
        }

        private long GetResponseContentLength(IDotvvmRequestContext request)
        {
            var dotvvmPresenter = request.Presenter as DotvvmPresenter;
            var diagnosticsRenderer = dotvvmPresenter?.OutputRenderer as DiagnosticsRenderer;
            if (diagnosticsRenderer != null)
            {
                return diagnosticsRenderer.ContentLength;
            }
            return 0;
        }
    }

}