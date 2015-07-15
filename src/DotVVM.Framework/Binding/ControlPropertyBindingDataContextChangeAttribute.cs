using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyBindingDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }
        public ControlPropertyBindingDataContextChangeAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control)
        {
            var property = DotvvmProperty.ResolveProperty(control.Metadata.Type, PropertyName);
            ResolvedPropertySetter propertyValue;
            if (control.Properties.TryGetValue(property, out propertyValue))
            {
                var binding = propertyValue as ResolvedPropertyBinding;
                if (binding == null) return dataContext;
                return binding.Binding.Expression.Type;
            }
            else return dataContext;
            //var property = control.Properties.FirstOrDefault(p => p.Key.Name == PropertyName).Value as ResolvedPropertyBinding;
            //if (property == null) throw new Exception($"property { PropertyName } does not exists on the control.");
            //return property.GetValue(control).GetType();
            throw new NotImplementedException();
        }
    }
}
