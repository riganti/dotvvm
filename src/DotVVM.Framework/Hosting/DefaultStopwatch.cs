using System.Diagnostics;

namespace DotVVM.Framework.Hosting
{
    public class DefaultStopwatch : IStopwatch
    {
        private Stopwatch stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStopwatch" /> class.
        /// </summary>
        public DefaultStopwatch()
        {
            stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Returns elapsed miliseconds in the stopwatch.
        /// </summary>
        public long GetElapsedMiliseconds()
        {
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
