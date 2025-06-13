using System;
using System.Collections.Concurrent;
using System.Resources;
using System.Threading;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Utils;

namespace DotVVM.AutoUI.Metadata
{
    public sealed class LocalizableString: IEquatable<LocalizableString>
    {
        public string? Value { get; private init; }
        public string? ResourceKey { get; private init; }
        public Type? ResourceType { get; private init; }
        private volatile ResourceManager? resourceManager;

        public bool IsLocalized => ResourceKey is {};

        private LocalizableString() { }

        public static LocalizableString Constant(string value) => new() { Value = value };
        public static LocalizableString Localized(Type type, string resourceKey)
        {
            ThrowHelpers.ArgumentNull(type);
            ThrowHelpers.ArgumentNull(resourceKey);
            return new() { ResourceKey = resourceKey, ResourceType = type };
        }


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
                // most likely, the same resource is used on multiple places; the init isn't too expensive, but duplicate bindings still take up quite some memory
                var binding = bindingCompilationService.Cache.CreateCachedBinding("DotVVM.AutoUI.Metadata.LocalizableString", [ this ], () =>
                    new ResourceBindingExpression<string>(
                        bindingCompilationService,
                        new object[] {
                            new ParsedExpressionBindingProperty(
                                ExpressionUtils.Replace(() => this.Localize())
                            ),
                            new ResultTypeBindingProperty(typeof(string)),
                            new ExpectedTypeBindingProperty(typeof(string)),
                            new OriginalStringBindingProperty($"{ResourceType.Name}.{ResourceKey}"), // make ToString more useful
                            (BindingDelegate)(_ => this.Localize()) // skip the expression compilation
                        }
                    )
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
            {
                var manager = this.resourceManager ??= GetResourceManager(ResourceType!);
                return manager.GetString(ResourceKey!) ?? Value ?? ResourceKey ?? "";
            }
        }

        public bool Equals(LocalizableString? other) =>
            other is not null &&
            other.Value == Value && other.ResourceType == ResourceType && other.ResourceKey == ResourceKey;
        public override bool Equals(object? other) =>
            other is LocalizableString otherLS && Equals(otherLS);
        public override int GetHashCode() =>
            ValueTuple.Create(Value?.GetHashCode() ?? 0, ResourceType?.GetHashCode() ?? 0, ResourceKey?.GetHashCode() ?? 0).GetHashCode();

        private static ConcurrentDictionary<Type, ResourceManager> resourceManagers = new(concurrencyLevel: 1, capacity: 4);
        private static ResourceManager GetResourceManager(Type type) =>
            resourceManagers.GetOrAdd(type, type => new ResourceManager(type));
    }
}
