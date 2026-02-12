using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Tracing
{
    public interface IStartupTracer
    {
        void TraceEvent(string eventName);

        Task NotifyStartupCompleted();

    }

}
