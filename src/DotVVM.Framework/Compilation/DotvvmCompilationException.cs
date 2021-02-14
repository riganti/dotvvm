#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Compilation
{
    [Serializable]
    public class DotvvmCompilationException : Exception
    {

        public string? FileName { get; set; }
        public string? SystemFileName => FileName?.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        public IEnumerable<TokenBase>? Tokens { get; set; }

        public int? ColumnNumber { get; set; }

        public int? LineNumber { get; set; }


        public DotvvmCompilationException(string message) : base(message) { }

        public DotvvmCompilationException(string message, Exception? innerException) : base(message, innerException) { }

        public DotvvmCompilationException(string message, Exception? innerException, IEnumerable<TokenBase>? tokens) : base(message, innerException)
        {
            if (tokens != null)
            {
                if (!(tokens is IList<TokenBase>)) tokens = tokens.ToArray();
                this.Tokens = tokens;
                LineNumber = tokens.FirstOrDefault()?.LineNumber;
                ColumnNumber = tokens.FirstOrDefault()?.ColumnNumber;
            }
        }

        public DotvvmCompilationException(string message, IEnumerable<TokenBase>? tokens) : this(message, null, tokens) { }
        protected DotvvmCompilationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
