using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Tracing;
using StackExchange.Profiling;
using System.Threading.Tasks;

namespace DotVVM.Tracing.MiniProfiler
{
    public interface IMiniProfilerRequestTracer
    {
        /// <summary>
        /// Returns an <see cref="Timing"/> (<see cref="IDisposable"/>) that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="profiler">The current profiling session or null.</param>
        /// <param name="name">A descriptive name for the code that is encapsulated by the resulting Timing's lifetime.</param>
        /// <returns>the profile step</returns>
        Timing Step(string name);

        /// <summary>
        ///     Returns an StackExchange.Profiling.Timing (System.IDisposable) that will time
        ///     the code between its creation and disposal. Will only save the StackExchange.Profiling.Timing
        ///     if total time taken exceeds minSaveMs.
        /// </summary>
        /// <param name="name">A descriptive name for the code that is encapsulated by the resulting Timing's lifetime.</param>
        /// <param name="minDuration">The minimum amount of time that needs to elapse in order for this result to be recorded.</param>
        /// <param name="includeChildren">
        ///  Should the amount of time spent in child timings be included when comparing total
        ///  time profiled with minSaveMs? If true, will include children. If false will ignore
        ///  children.
        ///</param>
        /// <returns>
        /// If includeChildren is set to true and a child is removed due to its use of StepIf,
        /// then the time spent in that time will also not count for the current StepIf calculation.
        /// </returns>
        Timing StepIf(string name, long minDuration, bool includeChildren = false);
    }
}