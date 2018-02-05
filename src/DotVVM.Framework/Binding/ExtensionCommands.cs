﻿using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Binding
{
    public static class ExtensionCommands
    {
        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Command action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Action action, string methodUsageId)
            => RegisterExtensionCommand(control, action, methodUsageId);

        public static CommandBindingExpression RegisterExtensionCommand(this DotvvmControl control, Delegate action, string methodUsageId)
        {
            var bindingService = control.GetValue(Internal.RequestContextProperty).CastTo<IDotvvmRequestContext>()
                .Configuration.ServiceProvider.GetRequiredService<BindingCompilationService>();
            var id = control.GetDotvvmUniqueId() + methodUsageId;
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.ResolveProperty(typeof(PropertyBox), propertyName) ?? DotvvmProperty.Register(propertyName, typeof(object), typeof(PropertyBox), null, false, null, typeof(PropertyBox));
            var binding = new CommandBindingExpression(bindingService, action, id);
            control.SetBinding(property, binding);
            return binding;
        }

        public static CommandBindingExpression GetExtensionCommand(this DotvvmControl control, string methodUsageId)
        {
            var propertyName = control.GetType().FullName + "/" + methodUsageId;
            var property = DotvvmProperty.ResolveProperty(typeof(PropertyBox), propertyName);
            return control.GetCommandBinding(property) as CommandBindingExpression;
        }

        class PropertyBox { }
    }
}
