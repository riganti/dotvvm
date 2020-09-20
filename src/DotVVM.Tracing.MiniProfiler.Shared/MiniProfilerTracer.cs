using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Profiling;

namespace DotVVM.Tracing.MiniProfiler
{
    using MiniProfiler = StackExchange.Profiling.MiniProfiler;
    public class MiniProfilerTracer : IRequestTracer, IMiniProfilerRequestTracer
    {
        public MiniProfilerTracer(IRequestTimingStorage storage)
        {
            this.storage = storage;
        }
        private IRequestTimingStorage storage;

        public Task EndRequest(IDotvvmRequestContext context)
        {
            SetEventNameStopCurrentTiming(RequestTracingConstants.EndRequest);
            return StopProfiler();
        }

        public Task EndRequest(IDotvvmRequestContext context, Exception exception)
        {
            SetEventNameStopCurrentTiming(RequestTracingConstants.EndRequest);
            return StopProfiler();
        }

        private Task StopProfiler()
        {
            return GetProfilerCurrent()?.StopAsync() ?? TaskUtils.GetCompletedTask();
        }

        private MiniProfiler GetProfilerCurrent()
        {
            return MiniProfiler.Current;
        }

        public Task TraceEvent(string eventName, IDotvvmRequestContext context)
        {
            EnsureProfilerStarted();

            SetEventNameStopCurrentTiming(eventName);

            storage.Current = (Timing)GetProfilerCurrent().Step(string.Empty);
            return TaskUtils.GetCompletedTask();
        }

        private void EnsureProfilerStarted()
        {
            if (GetProfilerCurrent() == null)
            {
                MiniProfiler.StartNew();
            }
        }

        private void SetEventNameStopCurrentTiming(string eventName)
        {
            if (storage.Current != null)
            {
                storage.Current.Name = eventName;
                storage.Current.Stop();
            }
        }

        public Timing Step(string name)
        {
            EnsureProfilerStarted();
            return GetProfilerCurrent().Step(name);
        }
    }
}