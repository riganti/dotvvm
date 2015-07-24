using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.ViewModel
{
    public class ControlTreeWalker
    {

        private DotvvmControl root;

        public Stack<string> CurrentPath { get; private set; }

        public string[] CurrentPathArray { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelJTokenControlTreeWalker"/> class.
        /// </summary>
        public ControlTreeWalker(DotvvmControl root)
        {
            this.root = root;
        }


        /// <summary>
        /// Processes the control tree.
        /// </summary>
        public void ProcessControlTree(Action<DotvvmControl> action)
        {
            CurrentPath = new Stack<string>();
            RefreshCurrentPathArray();

            ProcessControlTreeCore(root, action);
        }

        /// <summary>
        /// Refreshes the current path array.
        /// </summary>
        private void RefreshCurrentPathArray()
        {
            CurrentPathArray = CurrentPath.Reverse().ToArray();
        }

        /// <summary>
        /// Processes the control tree.
        /// </summary>
        private void ProcessControlTreeCore(DotvvmControl control, Action<DotvvmControl> action)
        {
            action(control);

            // if there is a DataContext binding, locate the correct token
            ValueBindingExpression binding;
            var hasDataContext = false;
            if (control is DotvvmBindableControl &&
                (binding = ((DotvvmBindableControl)control).GetBinding(DotvvmBindableControl.DataContextProperty, false) as ValueBindingExpression) != null)
            {
                CurrentPath.Push(binding.Javascript);
                RefreshCurrentPathArray();
                hasDataContext = true;
            }

            // go through all children
            foreach (var child in control.Children)
            {
                ProcessControlTreeCore(child, action);
            }

            if (hasDataContext)
            {
                CurrentPath.Pop();
                RefreshCurrentPathArray();
            }
        }

    }
}
