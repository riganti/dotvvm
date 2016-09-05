using System;
using System.Reflection;

namespace DotVVM.Framework.Pipeline
{
    public class NormalPipe<TResult, TPassable> : PipeBase<TResult, TPassable>
    {
        private readonly Type _class;
        private readonly string _action;
        private readonly PipeBase<TResult, TPassable> _next;

        public NormalPipe(Type @class, string action, PipeBase<TResult, TPassable> next)
        {
            _class = @class;
            _action = action;
            _next = next;
        }

        public override TResult Invoke(TPassable passable)
        {
            return (TResult)_class.GetMethod(_action).Invoke(Activator.CreateInstance(_class), new object[] { passable, _next.AsFunc() });
        }
    }
}