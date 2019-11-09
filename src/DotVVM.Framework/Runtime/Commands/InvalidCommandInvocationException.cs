#nullable enable
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Runtime.Commands
{
    public class InvalidCommandInvocationException : Exception
    {
        public Dictionary<string, string[]>? AdditionData { get; set; }

        public InvalidCommandInvocationException(string message)
            : base(message)
        {
            
        }

        public InvalidCommandInvocationException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        public InvalidCommandInvocationException(string message, Dictionary<string, CandidateBindings> data)
            : this(message, (Exception?)null, data)
        {

        }

        public InvalidCommandInvocationException(string message, Exception? innerException, Dictionary<string, CandidateBindings> data)
            : base(message, innerException)
        {
            if(data != null)
            {
                AdditionData = new Dictionary<string, string[]>();
                foreach (var bindings in data)
                {
                    AdditionData.Add(bindings.Key, bindings.Value.BindingsToString());
                }
            }

        }
    }
}
