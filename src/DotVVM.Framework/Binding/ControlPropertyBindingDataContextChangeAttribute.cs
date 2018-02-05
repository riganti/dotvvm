using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyBindingDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }

        public override int Order { get; }

        public bool AllowMissingProperty { get; set; }

        public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
        {
            if (!control.Metadata.TryGetProperty(PropertyName, out var controlProperty))
            {
                throw new Exception($"The property '{PropertyName}' was not found on control '{control.Metadata.Type}'!");
            }

            if (control.TryGetProperty(controlProperty, out var setter))
            {
                return setter is IAbstractPropertyBinding binding 
                    ? binding.Binding.ResultType 
                    : dataContext;
            }

            if (AllowMissingProperty)
            {
                return dataContext;
            }

            throw new Exception($"Property '{PropertyName}' is required on '{control.Metadata.Type.Name}'.");
        }

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty property = null)
        {
            var controlType = control.GetType();
            var controlPropertyField = controlType.GetField($"{PropertyName}Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var controlProperty = (DotvvmProperty)controlPropertyField?.GetValue(null);

            if (controlProperty == null)
            {
                throw new Exception($"The property '{PropertyName}' was not found on control '{controlType}'!");
            }

            if (control.Properties.ContainsKey(controlProperty))
            {
                var binding = control.GetValueBinding(controlProperty);
                return binding == null ? dataContext : binding.ResultType;
            }

            if (AllowMissingProperty)
            {
                return dataContext;
            }

            throw new Exception($"Property '{PropertyName}' is required on '{controlType.Name}'.");
        }

        public ControlPropertyBindingDataContextChangeAttribute(string propertyName, int order = 0)
        {
            PropertyName = propertyName;
            Order = order;
        }
        public override IEnumerable<string> PropertyDependsOn => new [] { PropertyName };
    }
}
