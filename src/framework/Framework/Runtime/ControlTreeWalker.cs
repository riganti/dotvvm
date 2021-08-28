using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime
{
    public class ControlTreeWalker
    {

        private DotvvmBindableObject root;

        public Stack<string> CurrentPath { get; } = new Stack<string>();

        public string[] CurrentPathArray { get; private set; } = new string[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlTreeWalker"/> class.
        /// </summary>
        public ControlTreeWalker(DotvvmBindableObject root)
        {
            this.root = root;
        }


        /// <summary>
        /// Processes the control tree.
        /// </summary>
        public void ProcessControlTree(Action<DotvvmBindableObject> action)
        {
            CurrentPath.Clear();
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
        private void ProcessControlTreeCore(DotvvmBindableObject control, Action<DotvvmBindableObject> action)
        {
            // if there is a DataContext binding, locate the correct token
            var hasDataContext = false;
            string? pathValue = null;
            try
            {
                // may throw exception if the control tree is not path-addressable
                pathValue = control.GetDataContextPathFragment();
            }
            catch(Exception) { return; }

            if (pathValue != null)
            {
                CurrentPath.Push(pathValue as string);
                RefreshCurrentPathArray();
                hasDataContext = true;
            }

            action(control);

            // go through all children
            foreach (var child in control.GetLogicalChildren())
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
