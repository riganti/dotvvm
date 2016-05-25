using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Binding
{
    public static class BindingHelper
    {
        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Command action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Action action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Delegate action, string methodUsageId)
        {
            var id = control.GetDotvvmUniqueId() + methodUsageId;
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.Register<object, ExtensionCommands>(propertyName);
            var binding = new CommandBindingExpression(action, id);
            control.SetBinding(property, binding);
            return binding;
        }

        public static CommandBindingExpression GetExtensionCommand(this DotvvmControl control, string methodUsageId)
        {
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.ResolveProperty(typeof(ExtensionCommands), propertyName);
            return control.GetCommandBinding(property) as CommandBindingExpression;
        }

        class ExtensionCommands { }
    }
}
