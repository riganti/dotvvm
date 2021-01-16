#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    internal class TextReportLogger : IReportLogger
    {
        public void Log(Stream stream, IEnumerable<Report> reports)
        {
            using var writer = new StreamWriter(stream);
            foreach (var report in reports)
            {
                writer.WriteLine($"{report.ViewPath}({report.Line},{report.Column}): {report.Message}");
            }
        }
    }
}
