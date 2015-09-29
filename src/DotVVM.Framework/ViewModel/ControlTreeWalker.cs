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
        /// Initializes a new instance of the <see cref="ControlTreeWalker"/> class.
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
            var hasDataContext = false;
            if (control is DotvvmBindableControl)
            {
                var pathValue = ((DotvvmBindableControl)control).GetValue(Internal.PathFragmentProperty, false);
                if (pathValue != null)
                {
                    CurrentPath.Push(pathValue as string);
                    RefreshCurrentPathArray();
                    hasDataContext = true;
                }
                else
                {
                    var binding = ((DotvvmBindableControl)control).GetValueBinding(DotvvmBindableControl.DataContextProperty, false);
                    if (binding != null)
                    {
                        CurrentPath.Push(binding.GetKnockoutBindingExpression());
                        RefreshCurrentPathArray();
                        hasDataContext = true;
                    }
                }
            }

            // go through all children
            foreach (var child in control.GetChildren())
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
