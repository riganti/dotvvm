using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser
{
    public class ParserException : Exception
    {

        public int LineNumber { get; set; }

        public int PositionOnLine { get; set; }

        public string FileName { get; set; }

        
        public ParserException(string message) : base(message)
        {
        }

        public ParserException(string message, string fileName) : base(message)
        {
            FileName = fileName;
        }

        public ParserException(string message, string fileName, int lineNumber)
            : this("Line " + lineNumber + ": " + message, fileName)
        {
            LineNumber = lineNumber;
        }
        
        public ParserException(string message, string fileName, int lineNumber, int columnNumber)
            : this("Line " + lineNumber + ", Column " + columnNumber + ": " + message, fileName)
        {
            LineNumber = lineNumber;
            PositionOnLine = columnNumber;
        }

    }
}
