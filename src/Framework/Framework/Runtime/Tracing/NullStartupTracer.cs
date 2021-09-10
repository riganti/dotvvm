using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime.Tracing
{
    public class NullStartupTracer : IStartupTracer
    {
        public Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender)
        {
            return TaskUtils.GetCompletedTask();
        }
        public void TraceEvent(string eventName)
        {
            
        }
    }
}
