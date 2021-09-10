using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IStartupTracer
    {
        void TraceEvent(string eventName);

        Task NotifyStartupCompleted(IDiagnosticsInformationSender informationSender);

    }

}
