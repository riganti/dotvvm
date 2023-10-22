using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.Commands
{
    public class InvalidCommandInvocationException : Exception
    {
        public KeyValuePair<string, string[]>[]? AdditionData { get; set; }

        public InvalidCommandInvocationException(string message)
            : base(message)
        {
            
        }

        public InvalidCommandInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        public InvalidCommandInvocationException(string message, KeyValuePair<string, string[]>[]? data)
            : this(message, (Exception?)null, data)
        {

        }

        public InvalidCommandInvocationException(string message, Exception? innerException, KeyValuePair<string, string[]>[]? data)
            : base(message, innerException)
        {
            AdditionData = data;
        }
    }
}
