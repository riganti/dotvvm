using DotVVM.Framework.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace DotVVM.Framework.Exceptions
{
    public class DotvvmCompilationException : ApplicationException
    {

        public string FileName { get; set; }

        public IEnumerable<TokenBase> Tokens { get; set; }

        public int? ColumnNumber { get; set; }

        public int? LineNumber { get; set; }


        public DotvvmCompilationException(string message) : base(message)
        {
        }

        public DotvvmCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DotvvmCompilationException(string message, Exception innerException, IEnumerable<TokenBase> tokens) : base(message, innerException)
        {
            this.Tokens = tokens;
            LineNumber = tokens.FirstOrDefault()?.LineNumber;
            ColumnNumber = tokens.FirstOrDefault()?.ColumnNumber;
        }
        public DotvvmCompilationException(string message, IEnumerable<TokenBase> tokens) : base(message)
        {
            this.Tokens = tokens;
            LineNumber = tokens.FirstOrDefault()?.LineNumber;
            ColumnNumber = tokens.FirstOrDefault()?.ColumnNumber;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(LineNumber), LineNumber);
            info.AddValue(nameof(ColumnNumber), ColumnNumber);
        }
    }
}
