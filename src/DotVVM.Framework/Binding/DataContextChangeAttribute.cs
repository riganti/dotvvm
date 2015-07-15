using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute: Attribute
    {
        public abstract Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control);

        public static Type GetDataContextType(DataContextStack dataContextStack, ResolvedControl control)
        {
            var attributes = control.Metadata.Type.GetCustomAttributes<DataContextChangeAttribute>();
            var dataContext = dataContextStack.DataContextType;
            foreach (var attribute in attributes)
            {
                dataContext = attribute.GetChildDataContextType(dataContext, dataContextStack, control);
            }
            return dataContext;
        }
    }
}
