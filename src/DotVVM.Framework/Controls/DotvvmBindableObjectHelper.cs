using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    public static class DotvvmBindableObjectHelper
    {
        public static DotvvmProperty GetDotvvmProperty<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
        {
            var property = (prop.Body as MemberExpression)?.Member as PropertyInfo;
            if (property == null) throw new Exception($"Expression '{prop}' should be property access on the specified control.");
            return DotvvmProperty.ResolveProperty(property.DeclaringType, property.Name) ?? throw new Exception($"Property '{property.DeclaringType.Name}.{property.Name}' is not a registered DotvvmProperty.");
        }

        public static TControl SetBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, IBinding binding)
            where TControl : DotvvmBindableObject
        {
            control.SetBinding(control.GetDotvvmProperty(prop), binding);
            return control;
        }

        public static TControl SetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, TProperty value)
            where TControl : DotvvmBindableObject
        {
            control.SetValue(control.GetDotvvmProperty(prop), value);
            return control;
        }

        public static TControl SetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop, ValueOrBinding<TProperty> value)
            where TControl : DotvvmBindableObject
        {
            control.SetValue(control.GetDotvvmProperty(prop), value);
            return control;
        }

        public static IValueBinding<TProperty> GetValueBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (IValueBinding<TProperty>)control.GetValueBinding(control.GetDotvvmProperty(prop));

        public static ICommandBinding<TProperty> GetCommandBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => (ICommandBinding<TProperty>)control.GetCommandBinding(control.GetDotvvmProperty(prop));

        public static TProperty GetValue<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => control.GetValue<TProperty>(control.GetDotvvmProperty(prop));

        public static ValueOrBinding<TProperty> GetValueOrBinding<TControl, TProperty>(this TControl control, Expression<Func<TControl, TProperty>> prop)
            where TControl : DotvvmBindableObject
            => control.GetValueOrBinding<TProperty>(control.GetDotvvmProperty(prop));
    }
}
