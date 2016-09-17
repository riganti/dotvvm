using System;

namespace DotVVM.CommandLine.Commands
{
    public class InvalidCommandUsageException : Exception
    {
        public InvalidCommandUsageException(string message)
        {
        }
    }
}