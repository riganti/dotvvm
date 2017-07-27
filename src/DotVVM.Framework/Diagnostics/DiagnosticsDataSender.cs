using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{
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
