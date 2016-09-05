using System;

namespace DotVVM.Framework.Pipeline
{
    public abstract class PipeBase<TResult, TPassable>
    {
        /// <summary>
        /// Invoke pipe processing.
        /// </summary>
        /// <param name="passable"></param>
        /// <returns></returns>
        public abstract TResult Invoke(TPassable passable);

        /// <summary>
        /// Get pipe as function.
        /// </summary>
        /// <returns></returns>
        public Func<TPassable, TResult> AsFunc() => Invoke;
    }
}