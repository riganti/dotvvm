using DotVVM.Framework.Compilation;

namespace DotVVM.Compiler
{
    public class Report
    {
        public Report(string viewPath, int line, int column, string message)
        {
            ViewPath = viewPath;
            Line = line;
            Column = column;
            Message = message;
        }

        public Report(DotvvmCompilationException exception)
            : this(exception.FileName, exception.LineNumber ?? -1, exception.ColumnNumber ?? -1, exception.Message)
        {
        }

        public string Message { get; }

        public int Line { get; }

        public int Column { get; }

        public string ViewPath { get; }
    }
}
