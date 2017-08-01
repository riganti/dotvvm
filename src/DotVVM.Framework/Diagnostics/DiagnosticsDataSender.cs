using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

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

    public interface IDiagnosticsDataSender
    {

        Task SendDataAsync(DiagnosticsData data);
    }

    internal class DiagnosticsDataSender : IDiagnosticsDataSender
    {
        private DotvvmDiagnosticsConfiguration configuration;

        internal DiagnosticsDataSender(DotvvmDiagnosticsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendDataAsync(DiagnosticsData data)
        {
            var hostname = configuration.DiagnosticsServerHostname;
            var port = configuration.DiagnosticsServerPort;
            if (hostname != null && port.HasValue)
            {
                using (var client = new TcpClient())
                {
                    try
                    {
                        await client.ConnectAsync(hostname, port.Value);
                        using (var stream = new StreamWriter(client.GetStream()))
                        {
                            await stream.WriteAsync(JsonConvert.SerializeObject(data));
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }
    }
}
