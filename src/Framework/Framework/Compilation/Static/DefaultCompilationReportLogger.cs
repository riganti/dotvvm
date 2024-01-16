
using System;
using System.Collections.Generic;
using System.IO;

namespace DotVVM.Framework.Compilation.Static
{
    internal class DefaultCompilationReportLogger : ICompilationReportLogger
    {
        public void Log(Stream stream, IEnumerable<DotvvmCompilationDiagnostic> diagnostics)
        {
            using var writer = new StreamWriter(stream);
            foreach (var d in diagnostics)
            {
                writer.WriteLine($"{d.Location}: {d.Severity.ToString().ToLowerInvariant()}: {d.Message}");
            }
        }
    }
}
