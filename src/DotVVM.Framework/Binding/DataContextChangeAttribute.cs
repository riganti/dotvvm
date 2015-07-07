using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute: Attribute
    {
        public abstract Type GetChildDataContextType(Type dataContext, Type parentDataContext, DotvvmControl control);

        public static Type GetDataContextType(Type parentDataContext, DotvvmControl control)
        {
            var dataContext = (control as DotvvmBindableControl)?.DataContext?.GetType() ?? parentDataContext;
            var attributes = control.GetType().GetCustomAttributes<DataContextChangeAttribute>();
            foreach (var attribute in attributes)
            {
                dataContext = attribute.GetChildDataContextType(dataContext, parentDataContext, control);
            }
            return dataContext;
        }
    }
}
