using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// Constants containing names of events for tracing a request.
    /// </summary>
    public class RequestTracingConstants
    {
        public static readonly string InitCompleted = "InitCompleted";
        public static readonly string BeginRequest = "BeginRequest";
        public static readonly string ViewInitialized = "ViewInitialized";
        public static readonly string ViewModelCreated = "ViewModelCreated";
        public static readonly string LoadCompleted = "LoadCompleted";
        public static readonly string ViewModelDeserialized = "ViewModelDeserialized";
        public static readonly string CommandExecuted = "CommandExecuted";
        public static readonly string PreRenderCompleted = "PreRenderCompleted";
        public static readonly string ViewModelSerialized = "ViewModelSerialized";
        public static readonly string OutputRendered = "OutputRendered";
        public static readonly string EndRequest = "EndRequest";
        public static readonly string StaticCommandExecuted = "StaticCommandExecuted";
    }
}
