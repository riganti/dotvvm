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
    public abstract class BindingExpression : IBinding, IMutableBinding
    {
        protected struct PropValue
        {
            public readonly object Value;
            public readonly Exception Error;

            public object GetValue(ErrorHandlingMode errorMode) =>
                Error == null ? Value :
                errorMode == ErrorHandlingMode.ReturnNull ? null :
                errorMode == ErrorHandlingMode.ReturnException ? Error :
                throw new AggregateException(Error);

            public PropValue(object value, Exception error = null)
            {
                if (value == null && error == null) throw new ArgumentNullException();
                this.Value = value;
                this.Error = error;
            }
        }

        private readonly ConcurrentDictionary<Type, PropValue> properties = new ConcurrentDictionary<Type, PropValue>();
        protected readonly BindingCompilationService bindingService;


        public BindingExpression(BindingCompilationService service, IEnumerable<object> properties)
        {
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

        public object GetProperty(Type type, ErrorHandlingMode errorMode = ErrorHandlingMode.ThrowException) =>
            properties.GetOrAdd(type, ComputeProperty).GetValue(errorMode);

        bool IMutableBinding.IsMutable => this.GetProperty<IsMutableBindingProperty>(ErrorHandlingMode.ReturnNull)?.IsMutable ?? false;
        void IMutableBinding.AddProperty(object property)
        {
            if (!((IMutableBinding)this).IsMutable) throw new InvalidOperationException("Binding is frozen, can add property.");
            if (this.properties.TryAdd(property.GetType(), new PropValue(property)))
                throw new InvalidOperationException("Property already exists.");
            foreach (var prop in properties)
            {
                // remove all error properties
                if (prop.Value.Error != null) properties.TryRemove(prop.Key, out var _);
            }
        }
    }
}
