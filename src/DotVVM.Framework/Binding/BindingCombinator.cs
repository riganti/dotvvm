using System.Runtime.CompilerServices;
using System;
using System.Linq;
using System.Collections.Generic;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using System.Linq.Expressions;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public static class BindingCombinator
    {
        static readonly ConditionalWeakTable<BindingCombinatorDescriptor, ConditionalWeakTable<IBinding, ConditionalWeakTable<IBinding, Lazy<IBinding>>>> combinationCache =
            new ConditionalWeakTable<BindingCombinatorDescriptor, ConditionalWeakTable<IBinding, ConditionalWeakTable<IBinding, Lazy<IBinding>>>>();

        public static IBinding GetCombination(this BindingCombinatorDescriptor descriptor, IBinding a, IBinding b)
        {
            return combinationCache
                .GetOrCreateValue(descriptor)
                .GetOrCreateValue(a)
                .GetValue(b, _ => new Lazy<IBinding>(() => descriptor.ComputeCombination(a, b)))
                .Value;
        }

        static IBinding CreateAndAlsoCombination(IBinding a, IBinding b) =>
            a.DeriveBinding(
                new ParsedExpressionBindingProperty(
                    Expression.AndAlso(
                        a.GetProperty<ParsedExpressionBindingProperty>().Expression,
                        b.GetProperty<ParsedExpressionBindingProperty>().Expression
                    )
                )
            );
        public static readonly BindingCombinatorDescriptor AndAlsoCombination = new BindingCombinatorDescriptor(CreateAndAlsoCombination);
        public static void AndAssignProperty(this DotvvmBindableObject obj, DotvvmProperty property, object value)
        {
            if (property.PropertyType != typeof(bool)) throw new NotSupportedException($"Can only AND boolean properties, {property} is of type {property.PropertyType}");
            if (!obj.IsPropertySet(property))
            {
                obj.SetValue(property, value);
            }
            else
            {
                if (value is bool b && !b)
                    obj.SetValue(property, false);
                else if (obj.GetValue(property) is bool b2 && b2)
                    obj.SetValue(property, value);
                else
                    obj.SetValue(property,
                        AndAlsoCombination.GetCombination(
                            obj.GetValue(property) as IBinding ??
                                throw new NotSupportedException($"A IBinding instance or bool was expected in property {property}, got {obj.GetValue(property)?.GetType().Name ?? "null"}"),
                            value as IBinding ??
                                throw new NotSupportedException($"A IBinding instance or bool was expected to AndAssign to property {property}, got {obj.GetValue(property)?.GetType().Name ?? "null"}")
                        )
                    );
            }
        }

        public class BindingCombinatorDescriptor
        {
            private readonly Func<IBinding, IBinding, IBinding> func;

            public BindingCombinatorDescriptor(Func<IBinding, IBinding, IBinding> func)
            {
                this.func = func;
            }
            public IBinding ComputeCombination(IBinding a, IBinding b) => func(a, b);
        }
    }
}
