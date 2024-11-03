using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Diagnostics.Models;

namespace DotVVM.Framework.Diagnostics
{

    public class DiagnosticsInformationSender : IDiagnosticsInformationSender
    {
        private DiagnosticsServerConfiguration configuration;

        public DiagnosticsInformationSender(DiagnosticsServerConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public DiagnosticsInformationSenderState State =>
            configuration.GetFreshHostName() is {} && configuration.Port != 0 ? DiagnosticsInformationSenderState.Full : DiagnosticsInformationSenderState.Disconnected;

        public async Task SendInformationAsync(DiagnosticsInformation information)
        {
            var hostname = configuration.GetFreshHostName();
            var port = configuration.GetFreshPort();
            if (hostname != null && port.HasValue)
            {
                using (var client = new TcpClient())
                {
                    try
                    {
                        await client.ConnectAsync(hostname, port.Value);
                        using (var stream = client.GetStream())
                        {
                            var settings = DefaultSerializerSettingsProvider.Instance.SettingsHtmlUnsafe;
                            await JsonSerializer.SerializeAsync(stream, information, settings);
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
