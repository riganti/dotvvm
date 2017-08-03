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

    internal class DiagnosticsInformationSender : IDiagnosticsInformationSender
    {
        private DotvvmDiagnosticsConfiguration configuration;

        internal DiagnosticsInformationSender(DotvvmDiagnosticsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendDataAsync(DiagnosticsInformation information)
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
                            await stream.WriteAsync(JsonConvert.SerializeObject(information));
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
