
using System;
using System.Collections.Generic;
using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    internal class DefaultCompilationReportLogger : ICompilationReportLogger
    {
        public void Log(Stream stream, IEnumerable<CompilationReport> reports)
        {
            using var writer = new StreamWriter(stream);
            foreach (var report in reports)
            {
                writer.WriteLine($"{report.ViewPath}({report.Line},{report.Column}): {report.Message}");
            }
        }
    }
}
