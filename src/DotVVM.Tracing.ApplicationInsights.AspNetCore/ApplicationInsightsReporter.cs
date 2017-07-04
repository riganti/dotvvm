using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsReporter : IRequestTracingReporter
    {
        private RequestTelemetry requestMetrics;

        public virtual Task StartTraceEvents()
        {
            requestMetrics = new RequestTelemetry();
            requestMetrics.Start();
            return Task.CompletedTask;
        }

        public virtual Task TraceEvents(IDotvvmRequestContext context, Dictionary<string, object> traceData)
        {
            TelemetryClient client = GetTelemetryClient(context);

            requestMetrics.Name = context.HttpContext.Request.Method + " " + context.HttpContext.Request.Path.Value;
            requestMetrics.Url = context.HttpContext.Request.Url;
            requestMetrics.ResponseCode = context.HttpContext.Response.StatusCode.ToString();
            foreach (string eventName in traceData.Keys)
            {
                requestMetrics.Metrics.Add(eventName, double.Parse(traceData[eventName].ToString()));
            }
            requestMetrics.Stop();
            client.TrackRequest(requestMetrics);
            return Task.CompletedTask;
        }

        public virtual Task TraceException(IDotvvmRequestContext context, Exception e)
        {
            var client = GetTelemetryClient(context);

            client.TrackException(e);
            return Task.CompletedTask;
        }

        private static TelemetryClient GetTelemetryClient(IDotvvmRequestContext context)
        {
            return context.Services.GetService<TelemetryClient>();
        }

    }
}
