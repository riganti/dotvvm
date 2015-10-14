using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public static class BindingHelper
    {
        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmBindableControl control, Command action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmBindableControl control, Action action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmBindableControl control, Delegate action, string methodUsageId)
        {
            control.EnsureControlHasId();
            var id = control.ID + methodUsageId;
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.Register<object, ExtensionCommands>(propertyName);
            var binding = new CommandBindingExpression(action, id);
            control.SetBinding(property, binding);
            return binding;
        }

        public static CommandBindingExpression GetExtensionCommand(this DotvvmBindableControl control, string methodUsageId)
        {
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.ResolveProperty(typeof(ExtensionCommands), propertyName);
            return control.GetCommandBinding(property) as CommandBindingExpression;
        }

        class ExtensionCommands { }
    }
}
