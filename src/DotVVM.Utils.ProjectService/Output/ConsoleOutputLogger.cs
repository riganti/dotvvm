using System;

namespace DotVVM.Utils.ProjectService.Output
{
    public class ConsoleOutputLogger : IOutputLogger
    {

        private string GetFormed(string message)
        {
            return $"{DateTime.Now:HH:mm:ss}: {message}";
        }

        public void WriteVerbose(string message)
        {
            WriteInfo(message, ConsoleColor.Gray);
        }

        public void WriteInfo(string message)
        {
            WriteInfo(message, ConsoleColor.Gray);
        }

        public void WriteInfo(string message, ConsoleColor color)
        {
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(GetFormed(message));
            Console.ForegroundColor = defaultColor;
        }

        public void WriteError(string message)
        {
            WriteInfo(message, ConsoleColor.Red);
        }

        public void WriteWarning(string message)
        {
            WriteInfo(message, ConsoleColor.Yellow);
        }

        public void WriteError(Exception e)
        {
            WriteError(e.ToString());
        }
    }
}