using System;

namespace DotVVM.Utils.ConfigurationHost.Output
{
    public interface IOutputLogger
    {
        void WriteVerbose(string message);
        void WriteInfo(string message);
        void WriteInfo(string message, ConsoleColor color);
        void WriteError(string message);
        void WriteWarning(string message);
        void WriteError(Exception e);

    }
}
