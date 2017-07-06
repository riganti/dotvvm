using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Hosting
{
    public class DefaultTraceData : ITraceData
    {
        public Dictionary<string, object> TraceData { get; private set; }

        private long lastStopwatchState;

        public DefaultTraceData()
        {
            TraceData = new Dictionary<string, object>();
            lastStopwatchState = 0;
        }
        public void AddTraceData(string eventName, IStopwatch stopwatch)
        {
            long nextStopwatchState = stopwatch.GetElapsedMiliseconds();
            TraceData.Add(eventName, nextStopwatchState - lastStopwatchState);
            lastStopwatchState = nextStopwatchState;
        }
    }
}
