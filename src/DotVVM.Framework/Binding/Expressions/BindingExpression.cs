#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Framework.Binding.Expressions
{
    [BindingCompilationRequirements(optional: new[] { typeof(BindingResolverCollection) })]
    public abstract class BindingExpression : IBinding, ICloneableBinding
    {
        struct PropValue
        {
            public readonly object? Value;
            public readonly Exception? Error;

            public object? GetValue(ErrorHandlingMode errorMode, Func<Exception, Exception> exceptionFactory) =>
                Error == null ? Value :
                errorMode == ErrorHandlingMode.ReturnNull ? null :
                errorMode == ErrorHandlingMode.ReturnException ? Error :
                throw exceptionFactory(Error);

            public PropValue(object? value, Exception? error = null)
            {
                if (value == null && error == null) throw new ArgumentNullException();
                this.Value = value;
                this.Error = error;
            }
        }

        private readonly ConcurrentDictionary<Type, PropValue> properties = new ConcurrentDictionary<Type, PropValue>();
        protected readonly BindingCompilationService bindingService;


        public BindingExpression(BindingCompilationService service, IEnumerable<object?> properties)
        {
            this.toStringValue = new Lazy<string>(() => {
                // using Lazy improves performance a bit and most importantly handles StackOverflowException that could occur when OriginalStringBindingProperty getter fails
                string value;
                try
                {
                    value =
                        this.GetProperty<OriginalStringBindingProperty>(ErrorHandlingMode.ReturnNull)?.Code ??
                        this.GetProperty<ParsedExpressionBindingProperty>(ErrorHandlingMode.ReturnNull)?.Expression?.ToString() ??
                        this.GetProperty<KnockoutExpressionBindingProperty>(ErrorHandlingMode.ReturnNull)?.Code?.ToString(o => new Compilation.Javascript.CodeParameterAssignment($"${o.GetHashCode()}", Compilation.Javascript.OperatorPrecedence.Max)) ??
                        this.GetProperty<KnockoutJsExpressionBindingProperty>(ErrorHandlingMode.ReturnNull)?.Expression?.ToString() ??
                        "... unrepresentable binding content ...";
                }
                catch (Exception ex)
                {
                    // Binding.ToString is used in error handling, so it should not fail
                    value = $"Unable to get binding string due to {ex.GetType().Name}: {ex.Message}";
                }
                return $"{{{this.GetType().Name}: {value}}}";
            });

            foreach (var prop in properties)
                if (prop != null) this.properties[prop.GetType()] = new PropValue(prop);
            this.bindingService = service;
            service.InitializeBinding(this);
        }

        PropValue ComputeProperty(Type propertyType)
        {
            try
            {
                var value = bindingService.ComputeProperty(propertyType, this);
                return value is Exception error ? new PropValue(null, error) : new PropValue(value);
            }
            catch (Exception ex)
            {
                return new PropValue(null, ex);
            }
        }

        private static Exception noResolversException = new Exception("There are no additional resolvers for this binding.");
        /// <summary>
        /// For performance reasons, derived bindings can set BindingResolverCollection to null to prevent runtime computation of the property which is somewhat costly
        /// </summary>
        protected void AddNullResolvers()
        {
            this.properties.TryAdd(typeof(BindingResolverCollection), new PropValue(null, noResolversException));
        }

        static Func<Exception, Exception> GetExceptionFactory(IBinding contextBinding, Type propType) =>
            innerException =>
            innerException is BindingPropertyException bpe && bpe.StackTrace == null && bpe.Binding == contextBinding && bpe.Property == propType ?
            new BindingPropertyException(bpe.Binding, bpe.Property, bpe.CoreMessage, bpe.InnerException) :
            new BindingPropertyException(contextBinding, propType, innerException);

        public object? GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException)
        {
            if (!properties.TryGetValue(type, out var result))
            {
                var r = ComputeProperty(type);
                if (r.Error != null)
                {
                    // overwrite previous error, this has a chance of being more descriptive (due to blocked recursion)
                    properties[type] = r;
                    result = r;
                }
                else
                {
                    // don't overwrite value, it has to be singleton
                    result = properties.GetOrAdd(type, r);
                }
            }
            return result.GetValue(errorMode, GetExceptionFactory(this, type));
        }


        Lazy<string> toStringValue;
        public override string ToString() => toStringValue.Value;

        IEnumerable<object> ICloneableBinding.GetAllComputedProperties()
        {
            return properties.Values
                .Where(p => p.Error == null)
                .Select(p => p.Value ?? throw null!);
        }
    }
}
