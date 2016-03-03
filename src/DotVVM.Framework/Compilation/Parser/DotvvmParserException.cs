using System;

namespace DotVVM.Framework.Compilation.Parser
{
    public class DotvvmParserException : Exception
    {

        public int LineNumber { get; set; }

        public int PositionOnLine { get; set; }

        public string FileName { get; set; }

        
        public DotvvmParserException(string message) : base(message)
        {
        }

        public DotvvmParserException(string message, string fileName) : base(message)
        {
            FileName = fileName;
        }

        public DotvvmParserException(string message, string fileName, int lineNumber)
            : this("Line " + lineNumber + ": " + message, fileName)
        {
            LineNumber = lineNumber;
        }
        
        public DotvvmParserException(string message, string fileName, int lineNumber, int columnNumber)
            : this("Line " + lineNumber + ", Column " + columnNumber + ": " + message, fileName)
        {
            LineNumber = lineNumber;
            PositionOnLine = columnNumber;
        }

    }
}
