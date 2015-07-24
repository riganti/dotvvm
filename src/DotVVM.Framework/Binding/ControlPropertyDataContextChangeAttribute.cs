using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class ControlPropertyDataContextChangeAttribute : DataContextChangeAttribute
    {
        public string PropertyName { get; set; }
        public ControlPropertyDataContextChangeAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        public override Type GetChildDataContextType(Type dataContext, Type parentDataContext, DotvvmControl control)
        {
            var type = control.GetType();
            var property = type.GetProperty(PropertyName);
            if (property == null) throw new Exception($"property { PropertyName } does not exists on { type.FullName }.");
            return property.GetValue(control).GetType();
        }
    }
}
