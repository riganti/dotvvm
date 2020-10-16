using DotVVM.Framework.Compilation;

namespace DotVVM.Compiler
{
    public class Report
    {
        private const string UnknownError = "An unknown error occurred. This is likely a bug in the compiler.";

        public Report(string viewPath, int line, int column, string message)
        {
            ViewPath = viewPath;
            Line = line;
            Column = column;
            Message = message;
        }

        public Report(string viewPath, DotvvmCompilationException exception)
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
