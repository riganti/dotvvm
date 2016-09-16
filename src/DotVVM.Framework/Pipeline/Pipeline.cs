using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Pipeline
{
    public class Pipeline<TPassable>
    {
        private List<Type> _pipes = new List<Type>();
        private TPassable _passable;
        private string _action = "Handle";

        /// <summary>
        /// Send passable object through pipes.
        /// </summary>
        /// <param name="passable"></param>
        /// <returns></returns>
        public Pipeline<TPassable> Send(TPassable passable)
        {
            _passable = passable;

            return this;
        }

        /// <summary>
        /// Set pipes through which will be the passable object sent.
        /// </summary>
        /// <param name="pipes"></param>
        /// <returns></returns>
        public Pipeline<TPassable> Through(List<Type> pipes)
        {
            _pipes = _pipes.Union(pipes).ToList();

            return this;
        }

        /// <summary>
        /// Set pipes through which will be the passable object sent.
        /// </summary>
        /// <param name="pipes"></param>
        /// <returns></returns>
        public Pipeline<TPassable> Through(params Type[] pipes)
        {
            return Through(pipes.ToList());
        }

        /// <summary>
        /// Set action on pipes.
        /// </summary>
        /// <param name="name"></param>
        public void Action(string name)
        {
            _action = name;
        }

        /// <summary>
        /// Set action after all pipes and run processing.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public TResult Then<TResult>(Func<TPassable, TResult> func)
        {
            PipeBase<TResult, TPassable> last = new FinalPipe<TResult, TPassable>(func);
            _pipes.Reverse();
            _pipes.ForEach(e => last = new NormalPipe<TResult, TPassable>(e, _action, last));

            return last.Invoke(_passable);
        }

        /// <summary>
        /// Set action after all pipes and run processing.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public void Then(Action<TPassable> func)
        {
            VoidPipeBase<TPassable> last = new VoidFinalPipe<TPassable>(func);
            _pipes.Reverse();
            _pipes.ForEach(e => last = new VoidNormalPipe<TPassable>(e, _action, last));

            last.Invoke(_passable);
        }
    }
}