using System;
using System.Linq.Expressions;
using System.Resources;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Metadata
{
    public class LocalizableString
    {
        public string? Value { get; private init; }
        public string? ResourceKey { get; private init; }
        public Type? ResourceType { get; private init; }

        public bool IsLocalized => ResourceKey is {};

        public static LocalizableString Constant(string value) => new() { Value = value };
        public static LocalizableString Localized(Type type, string resourceKey) => new() { ResourceKey = resourceKey, ResourceType = type };


        public static LocalizableString? CreateNullable(string? value, Type? resourceType) =>
            String.IsNullOrEmpty(value) ? null : Create(value, resourceType);
        public static LocalizableString Create(string value, Type? resourceType)
        {
            if (value is null) throw new ArgumentNullException(nameof(value), "LocalizableString.Create need a value - it is either the displayed value or the resource key");

            if (resourceType is null)
            {
                return Constant(value);
            }
            else
            {
                return Localized(resourceType, value);
            }
        }

        public ValueOrBinding<string> ToBinding(BindingCompilationService bindingCompilationService)
        {
            if (IsLocalized)
            {
                var binding = new ResourceBindingExpression<string>(
                    bindingCompilationService,
                    new object[] {
                        new ParsedExpressionBindingProperty(
                            ExpressionUtils.Replace(() => this.Localize())
                        ),
                        (BindingDelegate)((_, _) => this.Localize()) // skip the expression compilation
                    }
                );
                return new ValueOrBinding<string>(binding);
            }
            else
            {
                return new ValueOrBinding<string>(Value ?? "");
            }
        }

        public string Localize()
        {
            if (!IsLocalized)
                return Value ?? "";
            else
                return new ResourceManager(ResourceType!).GetString(ResourceKey!) ?? Value ?? ResourceKey ?? "";
        }
    }
}
