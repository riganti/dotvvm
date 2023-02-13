using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Runtime.Filters
{
    public class ActionInfo
    {
        public ActionInfo(ICommandBinding? binding, Func<object?> action, bool isControlCommand, MethodInfo? invokedMethod, string?[]? argumentPaths)
        {
            Binding = binding;
            IsControlCommand = isControlCommand;
            Action = action;
            InvokedMethod = invokedMethod;
            ArgumentPaths = argumentPaths;
        }

        public MethodInfo? InvokedMethod { get; set; }
        public string?[]? ArgumentPaths { get; set; }
        public ICommandBinding? Binding { get; set; }
        public bool IsControlCommand { get; internal set; }

        public Func<object?> Action { get; set; }
    }
}
