using System;
using System.Diagnostics;
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
        private readonly DiagnosticsDataSender dataSender;

        public DiagnosticsRequestTracer()
        {
            var configuration = new DotvvmDiagnosticsConfiguration();
            dataSender = new DiagnosticsDataSender(configuration);
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
            return dataSender.SendDataAsync(diagnosticsData);
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            stopwatch.Stop();
            var diagnosticsData = BuildDiagnosticsData(context);
            diagnosticsData.ResponseDiagnostics.StatusCode = 500;
            diagnosticsData.ResponseDiagnostics.ExceptionStackTrace = exception.ToString();
            return dataSender.SendDataAsync(diagnosticsData);
        }

        private DiagnosticsData BuildDiagnosticsData(IDotvvmRequestContext request)
        {
            return new DiagnosticsData
            {
                RequestDiagnostics = HttpRequestDiagnostics.FromDotvvmRequestContext(request),
                ResponseDiagnostics = HttpResponseDiagnostics.FromDotvvmRequestContext(request),
                TotalDuration = stopwatch.ElapsedMilliseconds
            };
        }
    }

}