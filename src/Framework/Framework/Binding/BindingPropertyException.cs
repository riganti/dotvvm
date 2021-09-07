using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding
{
    public class BindingPropertyException: Exception
    {
        [JsonIgnore]
        public IBinding Binding { get; }
        public Type[] PropertyPath { get; }
        [JsonIgnore]
        public Type Property => PropertyPath[0];
        public string? CoreMessage { get; }
        public bool IsRequiredProperty { get; }

        private Lazy<string> msg;
        public Exception[] AdditionalInnerExceptions { get; }
        [JsonIgnore]
        public IEnumerable<Exception> AllInnerExceptions =>
            InnerException is null ? Enumerable.Empty<Exception>() : new [] { InnerException }.Concat(AdditionalInnerExceptions);
        public override string Message => msg.Value;

        static string GetMessage(IBinding binding, Type[] properties, string? message, Exception? innerException, bool isRequiredProperty)
        {
            var coreMsg = (message, innerException) switch {
                (null, null) => ".",
                ({}, null)   => ", " + message,
                (null, {})   => ": " + innerException!.Message,
                ({}, {})     => $", {message}: {innerException!.Message}"
            };
            if (!coreMsg.EndsWith("."))
                coreMsg += ".";

            var introMsg =
                isRequiredProperty ?
                $"Could not initialize binding as it is missing a required property {properties[0].Name}" :
                $"Unable to get property {properties[0].Name}";

            var pathMsg = "";
            if (properties.Length > 1)
            {
                pathMsg = $" Property path: {string.Join(", ", properties.Select(p => p.Name))}";
                if (innerException is null)
                    pathMsg += " - adding any of those properties to the binding would fix the issue";
                pathMsg += ".";
            }

            var bindingMsg = binding is null ? "" : $" Binding: {binding}.";
            return introMsg + coreMsg + bindingMsg + pathMsg;
        }

        public BindingPropertyException(IBinding binding, Type property, string message) : this(binding, new [] { property }, message, new Exception[0]) { }
        public BindingPropertyException(IBinding binding, Type property, Exception? innerException) : this(binding, new [] { property }, null, new [] { innerException }) { }
        public BindingPropertyException(IBinding binding, Type[] propertyPath, string? message, Exception?[]? innerExceptions = null, bool isRequiredProperty = false) : base((string?)null, innerExceptions?.FirstOrDefault(e => e is object))
        {
            this.Binding = binding;
            this.PropertyPath = propertyPath;
            this.CoreMessage = message;
            this.IsRequiredProperty = isRequiredProperty;
            this.AdditionalInnerExceptions =
                (innerExceptions?.Except(new [] { null, InnerException }).ToArray() ?? new Exception[0])!;
            this.msg = new Lazy<string>(() => GetMessage(Binding, PropertyPath, CoreMessage, InnerException, isRequiredProperty));
        }

        public BindingPropertyException Clone(IEnumerable<Exception>? additionalExceptions = null, bool? isRequiredProperty = null)
        {
            additionalExceptions ??= Enumerable.Empty<Exception>();
            var n = new BindingPropertyException(Binding, PropertyPath, CoreMessage, AllInnerExceptions.Concat(additionalExceptions).ToArray(), isRequiredProperty ?? this.IsRequiredProperty);
            return n;
        }

        public BindingPropertyException Nest(Type property, IEnumerable<Exception>? additionalExceptions = null, bool? isRequiredProperty = null)
        {
            additionalExceptions ??= Enumerable.Empty<Exception>();
            if (property == this.Property)
                return Clone(additionalExceptions, isRequiredProperty);
            return new BindingPropertyException(Binding, new [] { property }.Concat(PropertyPath).ToArray(), CoreMessage, AllInnerExceptions.Concat(additionalExceptions).ToArray(), isRequiredProperty ?? false);
        }

        public static BindingPropertyException FromArgumentExceptions(IBinding binding, Type property, Exception[] exceptions, bool? isRequiredProperty = null)
        {
            if (exceptions.Length == 0)
                throw new InvalidOperationException();
            if (exceptions[0] is BindingPropertyException bpe && bpe.Binding == binding)
                return bpe.Nest(property, exceptions.Skip(1), isRequiredProperty);
            return new BindingPropertyException(binding, new [] { property }, null, exceptions, isRequiredProperty ?? false);
        }
    }
}
