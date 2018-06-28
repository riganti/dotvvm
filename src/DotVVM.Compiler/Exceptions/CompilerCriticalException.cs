using System;

namespace DotVVM.Compiler.Exceptions
{
    public class CompilerCriticalException : Exception
    {
        public CompilerCriticalException(string message) : base(message)
        {
        }
    }
}