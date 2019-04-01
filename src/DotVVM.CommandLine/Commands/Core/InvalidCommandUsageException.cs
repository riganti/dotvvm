using System;

namespace DotVVM.CommandLine.Commands.Core
{
    public class InvalidCommandUsageException : Exception
    {
        public InvalidCommandUsageException(string message): base(message)
        {
        }
    }
}
