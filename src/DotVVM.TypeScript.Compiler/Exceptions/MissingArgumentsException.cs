using System;
using System.Runtime.Serialization;

namespace DotVVM.TypeScript.Compiler.Exceptions
{
    [Serializable]
    public class MissingArgumentsException : Exception
    {
        public MissingArgumentsException()
        {
        }

        public MissingArgumentsException(string message) : base(message)
        {
        }

        public MissingArgumentsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MissingArgumentsException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
