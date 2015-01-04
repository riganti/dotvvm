using System;
using System.Collections.Generic;
using System.Linq; 
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework.ViewModel
{
    public abstract class ControlTreeWalker<T>
    {

        private T viewModel;
        private RedwoodControl root;

        public Stack<string> CurrentPath { get; private set; }

        public string[] CurrentPathArray { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelJTokenControlTreeWalker"/> class.
        /// </summary>
        public ControlTreeWalker(T viewModel, RedwoodControl root)
        {
            this.viewModel = viewModel;
            this.root = root;
        }


        /// <summary>
        /// Processes the control tree.
        /// </summary>
        public void ProcessControlTree(Action<T, RedwoodControl> action)
        {
            CurrentPath = new Stack<string>();
            RefreshCurrentPathArray();
            var evaluator = CreateEvaluator(viewModel);

            ProcessControlTreeCore(evaluator, viewModel, root, action);
        }

        /// <summary>
        /// Refreshes the current path array.
        /// </summary>
        private void RefreshCurrentPathArray()
        {
            CurrentPathArray = CurrentPath.Reverse().ToArray();
        }

        /// <summary>
        /// Creates the expression evaluator.
        /// </summary>
        protected abstract IExpressionEvaluator<T> CreateEvaluator(T viewModel);

        /// <summary>
        /// Determines whether the walker shoulds the process children for current viewModel value.
        /// </summary>
        protected abstract bool ShouldProcessChildren(T viewModel);


        /// <summary>
        /// Processes the control tree.
        /// </summary>
        private void ProcessControlTreeCore(IExpressionEvaluator<T> evaluator, T viewModel, RedwoodControl control, Action<T, RedwoodControl> action)
        {
            action(viewModel, control);

            // if there is a DataContext binding, locate the correct token
            ValueBindingExpression binding;
            var hasDataContext = false;
            if (control is RedwoodBindableControl && 
                (binding = ((RedwoodBindableControl)control).GetBinding(RedwoodBindableControl.DataContextProperty, false) as ValueBindingExpression) != null)
            {
                viewModel = evaluator.Evaluate(binding.Expression);
                CurrentPath.Push(binding.GetViewModelPathExpression((RedwoodBindableControl)control, RedwoodBindableControl.DataContextProperty));
                RefreshCurrentPathArray();
                hasDataContext = true;
            }

            if (ShouldProcessChildren(viewModel))
            {
                // go through all children
                foreach (var child in control.Children)
                {
                    ProcessControlTreeCore(evaluator, viewModel, child, action);
                }
            }

            if (hasDataContext)
            {
                CurrentPath.Pop();
                RefreshCurrentPathArray();
            }
        }

    }
}
