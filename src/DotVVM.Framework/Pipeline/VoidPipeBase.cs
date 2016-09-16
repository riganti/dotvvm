using System;

namespace DotVVM.Framework.Pipeline
{
    public abstract class VoidPipeBase<TPassable>
    {
        /// <summary>
        /// Invoke pipe processing.
        /// </summary>
        /// <param name="passable"></param>
        /// <returns></returns>
        public abstract void Invoke(TPassable passable);

        /// <summary>
        /// Get pipe as function.
        /// </summary>
        /// <returns></returns>
        public Action<TPassable> AsFunc() => Invoke;
    }
}