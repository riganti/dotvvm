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
        public Type Property { get; }
        public string? CoreMessage { get; }
        private Lazy<string> msg;
        public override string Message => msg.Value;

        static string GetMessage(IBinding binding, Type property, string? message, Exception? innerException)
        {
            var m = $"Unable to get property {property.Name} of binding {binding?.ToString() ?? "{Unknown binding}"}";
            var suffix = innerException == null ? "." : ". (" + innerException.Message + ")";
            if (message == null) return m + suffix;
            else return m + ", " + message + suffix;
        }

        public BindingPropertyException(IBinding binding, Type property, string message) : this(binding, property, message, (Exception?)null) { }
        public BindingPropertyException(IBinding binding, Type property, Exception? innerException) : this(binding, property, null, innerException) { }
        public BindingPropertyException(IBinding binding, Type property, string? message, Exception[] innerExceptions)
            : this(
                binding,
                property,
                message,
                innerExceptions.Length > 1 ? new AggregateException(innerExceptions) : innerExceptions.SingleOrDefault()
            ) { }
        public BindingPropertyException(IBinding binding, Type property, string? message, Exception? innerException) : base((string?)null, innerException)
        {
            this.Binding = binding;
            this.Property = property;
            this.CoreMessage = message;
            this.msg = new Lazy<string>(() => GetMessage(Binding, Property, CoreMessage, innerException));
        }
    }
}
