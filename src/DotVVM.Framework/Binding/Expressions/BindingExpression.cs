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

            public object GetValue(bool optional) => Error == null || optional ? Value : throw new AggregateException(Error);

            public PropValue(object value, Exception error = null)
            {
                this.Value = value;
                this.Error = error;
            }
        }

        private readonly ConcurrentDictionary<Type, PropValue> properties = new ConcurrentDictionary<Type, PropValue>();
        protected readonly BindingCompilationService bindingService;


        public BindingExpression(BindingCompilationService service, IEnumerable<object> properties)
        {
            foreach (var prop in properties)
                this.properties.TryAdd(prop.GetType(), new PropValue(prop));
            this.bindingService = service;
            service.InitializeBinding(this);
        }

        PropValue ComputeProperty(Type propertyType)
        {
            try
            {
                return new PropValue(bindingService.ComputeProperty(propertyType, this));
            }
            catch (Exception ex)
            {
                return new PropValue(null, ex);
            }
        }

        public object GetProperty(Type type, bool optional = false) => properties.GetOrAdd(type, ComputeProperty).GetValue(optional);

        bool IMutableBinding.IsMutable => this.GetProperty<IsMutableBindingProperty>(optional: true)?.IsMutable ?? false;
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
