using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Exceptions
{
    public class DotvvmCompilationException : ApplicationException
    {

        public string FileName { get; set; }

        public int? ColumnNumber { get; set; }

        public int? LineNumber { get; set; }


        public DotvvmCompilationException(string message) : base(message)
        {
        }

        public DotvvmCompilationException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
