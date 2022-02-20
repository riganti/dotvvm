using System;
using System.Linq.Expressions;
using System.Resources;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;

namespace DotVVM.Framework.Controls.DynamicData.Metadata
{
    public class LocalizableString
    {
        public string? Value { get; private init; }
        public string? ResourceKey { get; private init; }
        public Type? ResourceType { get; private init; }

        public bool IsLocalized => ResourceKey is {};

        public static LocalizableString Constant(string value) => new() { Value = value };
        public static LocalizableString Localized(Type type, string resourceKey) => new() { ResourceKey = resourceKey, ResourceType = type };


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

        public ValueOrBinding<string> ToBinding(DynamicDataContext context)
        {
            if (IsLocalized)
            {
                var binding = new ResourceBindingExpression<string>(
                    context.BindingService,
                    new object[] {
                        new ParsedExpressionBindingProperty(
                            Expression.Call(
                                Expression.Constant(this),
                                "Localize",
                                Type.EmptyTypes)
                        ),
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
