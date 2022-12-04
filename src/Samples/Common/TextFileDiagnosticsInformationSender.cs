using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Configuration;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.Common
{
    public class TextFileDiagnosticsInformationSender : IDiagnosticsInformationSender
    {
        private readonly DotvvmConfiguration config;
        private readonly string logFilePath;
        private readonly object locker = new object();

        public TextFileDiagnosticsInformationSender(DotvvmConfiguration config)
        {
            this.config = config;

            logFilePath = GetFilePath();
        }

        public Task SendInformationAsync(DiagnosticsInformation information)
        {
            var messages = FormatUnwrittenMessages(information);

            lock (locker)
            {
                if (!File.Exists(logFilePath))
                {
                    messages = $@"{"Event Name",-70}{"Duration [ms]",15}{"Total [ms]",15}
{new string('-', 70 + 15 + 15)}
{messages}";
                }

                File.AppendAllText(logFilePath, messages);
            }

            return Task.CompletedTask;
        }

        private string FormatUnwrittenMessages(DiagnosticsInformation information)
        {
            var sb = new StringBuilder();
            foreach (var timing in information.EventTimings)
            {
                sb.AppendLine(FormatMessage(timing));
            }

            return sb.ToString();
        }

        protected virtual string FormatMessage(EventTiming timing)
        {
            return $"{timing.EventName,-70}{timing.Duration + " ms",15}{timing.TotalDuration + " ms",15}";
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
