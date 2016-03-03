using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyBindingDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }

        public override int Order { get; }

        public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
        {
            IPropertyDescriptor controlProperty;
            if (!control.Metadata.TryGetProperty(PropertyName, out controlProperty))
            {
                throw new Exception($"The property '{PropertyName}' was not found on control '{control.Metadata.Type}'!");
            }

            IAbstractPropertySetter setter;
            if (control.TryGetProperty(controlProperty, out setter))
            {
                var binding = setter as IAbstractPropertyBinding;
                if (binding == null)
                {
                    return dataContext;
                }
                return binding.Binding.ResultType;
            }
            else
            {
                return dataContext;
            }
        }

        public ControlPropertyBindingDataContextChangeAttribute(string propertyName, int order = 0)
        {
            PropertyName = propertyName;
            Order = order;
        }
        
    }
}
