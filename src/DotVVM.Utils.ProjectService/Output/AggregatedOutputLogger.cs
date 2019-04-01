using System;

namespace DotVVM.Utils.ProjectService.Output
{
    internal class AggregatedOutputLogger : IOutputLogger
    {
        public IOutputLogger[] Logger { get; }

        public AggregatedOutputLogger(params IOutputLogger[] logger)
        {
            Logger = logger;
        }

        public void WriteVerbose(string message)
        {
            foreach (var outputLogger in Logger)
            {
                outputLogger.WriteVerbose(message);
            }
        }

        public void WriteInfo(string message)
        {
            foreach (var outputLogger in Logger)
            {
                outputLogger.WriteInfo(message);
            }
        }

        public void WriteInfo(string message, ConsoleColor color)
        {
            foreach (var outputLogger in Logger)
            {
                outputLogger.WriteInfo(message, color);
            }
        }

        public void WriteError(string message)
        {
            foreach (var outputLogger in Logger)
            {
                outputLogger.WriteError(message);
            }
        }

        public void WriteError(Exception e)
        {
            WriteError(e.ToString());
        }

        public void WriteWarning(string message)
        {
            foreach (var outputLogger in Logger)
            {
                outputLogger.WriteWarning(message);
            }
        }

      
    }
}