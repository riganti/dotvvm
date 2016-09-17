using System;
using System.Reflection;

namespace DotVVM.Framework.Pipeline
{
    public class VoidNormalPipe<TPassable> : VoidPipeBase<TPassable>
    {
        private readonly Type _class;
        private readonly string _action;
        private readonly VoidPipeBase<TPassable> _next;

        public VoidNormalPipe(Type @class, string action, VoidPipeBase<TPassable> next)
        {
            _class = @class;
            _action = action;
            _next = next;
        }

        public override void Invoke(TPassable passable)
        {
            _class.GetMethod(_action).Invoke(Activator.CreateInstance(_class), new object[] { passable, _next.AsFunc() });
        }
    }
}