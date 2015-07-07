using Redwood.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Redwood.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute: Attribute
    {
        public abstract Type GetChildDataContextType(Type dataContext, Type parentDataContext, RedwoodControl control);

        public static Type GetDataContextType(Type parentDataContext, RedwoodControl control)
        {
            var dataContext = (control as RedwoodBindableControl)?.DataContext?.GetType() ?? parentDataContext;
            var attributes = control.GetType().GetCustomAttributes<DataContextChangeAttribute>();
            foreach (var attribute in attributes)
            {
                dataContext = attribute.GetChildDataContextType(dataContext, parentDataContext, control);
            }
            return dataContext;
        }
    }
}
