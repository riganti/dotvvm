using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Diagnostics.Models;
using Newtonsoft.Json;

namespace DotVVM.Framework.Diagnostics
{

    public class DiagnosticsInformationSender : IDiagnosticsInformationSender
    {
        private DotvvmDiagnosticsConfiguration configuration;

        public DiagnosticsInformationSender(DotvvmDiagnosticsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendInformationAsync(DiagnosticsInformation information)
        {
            var hostname = configuration.GetDiagnosticsServerHostname();
            var port = configuration.GetDiagnosticsServerPort();
            if (hostname != null && port.HasValue)
            {
                using (var client = new TcpClient())
                {
                    try
                    {
                        await client.ConnectAsync(hostname, port.Value);
                        using (var stream = new StreamWriter(client.GetStream()))
                        {
                            var settings = DefaultSerializerSettingsProvider.Instance.Settings;
                            await stream.WriteAsync(JsonConvert.SerializeObject(information, settings));
                            await stream.FlushAsync();
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
