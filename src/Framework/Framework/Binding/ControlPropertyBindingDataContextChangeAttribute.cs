using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyBindingDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }

        public override int Order { get; }

        public bool AllowMissingProperty { get; set; }

        public ControlPropertyBindingDataContextChangeAttribute(string propertyName, int order = 0)
        {
            PropertyName = propertyName;
            Order = order;
        }

        public override ITypeDescriptor? GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor? property = null)
        {
            if (!control.Metadata.TryGetProperty(PropertyName, out var controlProperty))
            {
                throw new Exception($"The property '{PropertyName}' was not found on control '{control.Metadata.Type}'!");
            }

            [DoesNotReturn]
            void ThrowDataContextMismatch(IAbstractBinding binding)
            {
                var sb = new StringBuilder();
                sb.Append($"The '{controlProperty.Name}' property contains invalid data-binding. ");
                sb.Append($"Binding '{binding}' could not be constructed on current context ({dataContext})");
                throw new Exception(sb.ToString());
            }

            if (control.TryGetProperty(controlProperty, out var setter))
            {
                if (setter is IAbstractPropertyBinding binding)
                {
                    if (binding.Binding.ResultType == null)
                        ThrowDataContextMismatch(binding.Binding);

                    return binding.Binding.ResultType;
                }

                return dataContext;
            }

            if (AllowMissingProperty)
            {
                return dataContext;
            }

            throw new Exception($"Property '{PropertyName}' is required on '{control.Metadata.Type.CSharpName}'.");
        }

        public override Type? GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty? property = null)
        {
            var controlType = control.GetType();
            var controlPropertyField = controlType.GetField($"{PropertyName}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var controlProperty = (DotvvmProperty?)controlPropertyField?.GetValue(null);

            if (controlProperty == null)
            {
                throw new Exception($"The property '{PropertyName}' was not found on control '{controlType}'!");
            }

            if (control.properties.Contains(controlProperty))
            {
                return control.GetValueBinding(controlProperty) is IValueBinding valueBinding
                    ? valueBinding.ResultType
                    : dataContext;
            }

            if (AllowMissingProperty)
            {
                return dataContext;
            }

            throw new Exception($"Property '{PropertyName}' is required on '{controlType.Name}'.");
        }

        public override IEnumerable<string> PropertyDependsOn => new[] { PropertyName };
    }
}
