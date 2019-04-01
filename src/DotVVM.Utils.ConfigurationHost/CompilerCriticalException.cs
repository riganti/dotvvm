using System;

namespace DotVVM.Utils.ConfigurationHost
{
    public class CompilerCriticalException : Exception
    {
        public CompilerCriticalException(string message) : base(message)
        {
        }
    }
}
