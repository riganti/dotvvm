using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Configuration;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using System;
using System.IO;
using System.Linq;

namespace DotVVM.Samples.Common
{
    public class TextFileDiagnosticsInformationSender : IDiagnosticsInformationSender
    {
        private readonly DotvvmConfiguration config;

        public TextFileDiagnosticsInformationSender(DotvvmConfiguration config)
        {
            this.config = config;
        }

        public Task SendInformationAsync(DiagnosticsInformation information)
        {
            var path = GetFilePath();
            var message = FormatMessage(information);

            File.WriteAllText(path, message);
            return Task.CompletedTask;
        }

        private string FormatMessage(DiagnosticsInformation information)
        {
            return $@"Total duration: {information.TotalDuration}ms
======
{string.Join(Environment.NewLine, information.EventTimings.Select(t => $"{t.EventName.PadRight(50)}{t.Duration.ToString().PadLeft(10)}"))}";
        }

        private string GetFilePath()
        {
            var directory = Path.Combine(config.ApplicationPhysicalPath, "obj");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var path = Path.Combine(directory, $"startup_{DateTime.Now:yyyy-MM-dd_HHmmss}");
            return path;
        }
    }
}
