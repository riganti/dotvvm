using System;

namespace DotVVM.Framework.Pipeline
{
    public class VoidFinalPipe<TPassable> : VoidPipeBase<TPassable>
    {
        private readonly Action<TPassable> _func;

        public VoidFinalPipe(Action<TPassable> func)
        {
            _func = func;
        }

        public override void Invoke(TPassable passable)
        {
            _func(passable);
        }
    }
}