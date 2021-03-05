#nullable enable

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
    }
}
