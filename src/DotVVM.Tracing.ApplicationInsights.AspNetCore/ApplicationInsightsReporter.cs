using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace DotVVM.Tracing.ApplicationInsights
{
    public class ApplicationInsightsReporter : IRequestTracingReporter
    {
        private RequestTelemetry requestMetrics;

        public Task StartTraceEvents()
        {
            requestMetrics = new RequestTelemetry();
            requestMetrics.Start();
            return Task.CompletedTask;
        }

        public Task TraceEvents(IDotvvmRequestContext context, Dictionary<string, object> traceData)
        {
            var client = context.Configuration.ServiceLocator.GetService<TelemetryClient>();

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

        public Task TraceException(IDotvvmRequestContext context, Exception e)
        {
            var client = context.Configuration.ServiceLocator.GetService<TelemetryClient>();

            client.TrackException(e);
            return Task.CompletedTask;
        }
    }
}
