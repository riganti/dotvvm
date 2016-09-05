using System;

namespace DotVVM.Framework.Pipeline
{
    public class FinalPipe<TResult, TPassable> : PipeBase<TResult, TPassable>
    {
        private readonly Func<TPassable, TResult> _func;

        public FinalPipe(Func<TPassable, TResult> func)
        {
            _func = func;
        }

        public override TResult Invoke(TPassable passable)
        {
            return _func(passable);
        }
    }
}