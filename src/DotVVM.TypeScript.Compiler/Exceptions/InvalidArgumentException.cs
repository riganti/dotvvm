using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Exceptions
{
    [Serializable]
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException()
        {
        }

        public InvalidArgumentException(string message) : base(message)
        {
        }

        public InvalidArgumentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidArgumentException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }}
