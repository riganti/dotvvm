
using System;

namespace DotVVM.Utils.ConfigurationHost
{
    // This interface is
    public interface ILogger
    {
        void LogInfo(string message);
        void LogException(Exception exception);
        void LogError(string message);
    }

    public enum LoggingSeverity
    {
        /// <summary>
        /// Deep diagnostics loading information.
        /// </summary>
        Diagnostics,
        /// <summary>
        /// Information that does not indicate a problem (i.e. not proscriptive).
        /// </summary>
        Info,
        /// <summary>Something suspicious but allowed.</summary>
        Warning,
        /// <summary>
        /// Something not allowed by the rules of the language or other authority.
        /// </summary>
        Error,
    }

}
