namespace DotVVM.TypeScript.Compiler.Utils.Logging
{
    public interface ILogger
    {
        void LogDebug(string category, string message);
        void LogInfo(string category, string message);
        void LogError(string category, string message);
    }
}
