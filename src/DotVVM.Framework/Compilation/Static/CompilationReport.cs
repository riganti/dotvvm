#nullable enable

using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Static
{
    internal class CompilationReport
    {
        private const string UnknownError = "An unknown error occurred. This is likely a bug in the compiler.";

        public CompilationReport(string viewPath, int line, int column, string message)
        {
            ViewPath = viewPath;
            Line = line;
            Column = column;
            Message = message;
        }

        public CompilationReport(string viewPath, DotvvmCompilationException exception)
            : this(
                viewPath: viewPath,
                line: exception.LineNumber ?? -1,
                column: exception.ColumnNumber ?? -1,
                message: !string.IsNullOrEmpty(exception.Message)
                    ? exception.Message
                    : exception.InnerException?.ToString() ?? UnknownError)
        {
        }

        public string Message { get; }

        public int Line { get; }

        public int Column { get; }

        public string ViewPath { get; }

        public static bool operator ==(CompilationReport? left, CompilationReport? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(CompilationReport? left, CompilationReport? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is CompilationReport report
                && Message == report.Message
                && Line == report.Line
                && Column == report.Column
                && ViewPath == report.ViewPath;
        }

        public override int GetHashCode()
        {
            var hashCode = -712964631;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ViewPath);
            return hashCode;
        }
    }
}
