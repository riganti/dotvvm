using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Hosting
{
    public interface ITraceData
    {
        Dictionary<string, object> TraceData { get; }
        
        void AddTraceData(string eventName, IStopwatch stopwatch);
    }
}
