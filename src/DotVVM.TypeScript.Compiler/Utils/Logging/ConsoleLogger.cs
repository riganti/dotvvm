using System;

namespace DotVVM.TypeScript.Compiler.Utils.Logging
{
    public class ConsoleLogger : ILogger
    {
        private const string PrefixDebug = "DEBUG";
        private const string PrefixInfo = "INFO";
        private const string PrefixError = "ERROR";

        public void LogDebug(string category, string message)
        {
#if DEBUG
            Log(ConsoleColor.Yellow, PrefixDebug, category, message);
#endif
        }

        public void LogInfo(string category, string message)
        {
            Log(ConsoleColor.White, PrefixInfo, category, message);
        }

        public void LogError(string category, string message)
        {
            Log(ConsoleColor.Red, PrefixError, category, message);
        }

        private void Log(ConsoleColor color, string prefix, string category, string message)
        {
            Console.ForegroundColor = color;
            var output = $"[{prefix} - {category}] {message}";
            Console.WriteLine(output);
            Console.ResetColor();
        }
    }
}
