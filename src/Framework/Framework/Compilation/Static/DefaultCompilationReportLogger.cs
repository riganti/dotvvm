
using System;
using System.Collections.Generic;
using System.IO;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Compilation.Static
{
    internal class DefaultCompilationReportLogger : ICompilationReportLogger
    {
        private const string Reset = "\u001b[0m";
        private const string BoldCyan = "\u001b[1;36m";
        private const string CornflowerBlue = "\u001b[38;2;100;149;237m";
        private const string Red = "\u001b[31m";
        private const string Yellow = "\u001b[33m";
        private const string LightYellow = "\u001b[93m";

        private readonly bool useColor;

        public DefaultCompilationReportLogger(bool useColor = true)
        {
            this.useColor = useColor;
        }

        public void Log(Stream stream, IEnumerable<DotvvmCompilationDiagnostic> diagnostics)
        {
            using var writer = new StreamWriter(stream);
            foreach (var d in diagnostics)
            {
                var severity = d.Severity.ToString().ToLowerInvariant();
                var severityColor = d.Severity == DiagnosticSeverity.Error ? Red : Yellow;

                WriteColored(writer, d.Location.ToString(), BoldCyan);
                writer.Write(": ");
                WriteColored(writer, severity, severityColor);
                writer.Write(": ");
                WriteMessage(writer, d.Message);

                var affectedSpan = string.Join("; ", d.Location.AffectedSpans);
                if (!string.IsNullOrWhiteSpace(affectedSpan))
                {
                    WriteAffectedSpan(writer, affectedSpan, d.Location.LineNumber);
                }
            }
        }

        private static void WriteMessage(StreamWriter writer, string message)
        {
            writer.WriteLine(message);
        }

        private void WriteAffectedSpan(StreamWriter writer, string affectedSpan, int? firstLineNumber)
        {
            var lines = affectedSpan.TrimEnd().Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var formattedLineNumber = firstLineNumber is { } firstLineAtIndex
                    ? (firstLineAtIndex + i).ToString()
                    : "?";
                writer.Write("  ");
                formattedLineNumber = formattedLineNumber.PadLeft(3);
                WriteColored(writer, formattedLineNumber, CornflowerBlue);
                writer.Write(": ");
                WriteColored(writer, lines[i], LightYellow);
                writer.WriteLine();
            }

            writer.WriteLine();
        }

        private void WriteColored(StreamWriter writer, string value, string color)
        {
            if (useColor)
                writer.Write(color);
            writer.Write(value);
            if (useColor)
                writer.Write(Reset);
        }
    }
}
