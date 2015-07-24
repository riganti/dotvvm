using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Runtime.Compilation;
using System.Linq.Expressions;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyBindingDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }

        public override int ParameterCount => 0;

        public ControlPropertyBindingDataContextChangeAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control, DotvvmProperty dproperty = null)
        {
            var property = DotvvmProperty.ResolveProperty(control.Metadata.Type, PropertyName);
            ResolvedPropertySetter propertyValue;
            if (control.Properties.TryGetValue(property, out propertyValue))
            {
                var binding = propertyValue as ResolvedPropertyBinding;
                if (binding == null) return dataContext;
                return binding.Binding.GetExpression().Type;
            }
            else return dataContext;
        }
    }
}
