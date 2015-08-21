using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Runtime.Compilation;
using System.Linq.Expressions;
using DotVVM.Framework.Parser;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute : Attribute
    {
        public abstract int Order { get; }
        public abstract Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control, DotvvmProperty property = null);

        public static Type GetDataContextExpression(DataContextStack dataContextStack, ResolvedControl control, DotvvmProperty property = null)
        {
            var attributes = property == null ? control.Metadata.Type.GetCustomAttributes<DataContextChangeAttribute>() : property.PropertyInfo?.GetCustomAttributes<DataContextChangeAttribute>();
            if (attributes == null) return null;
            var type = dataContextStack.DataContextType;
            var paramDataContextExpression = type;
            foreach (var attribute in attributes.OrderBy(a => a.Order))
            {
                type = attribute.GetChildDataContextType(type, dataContextStack, control, property);
            }
            return type == paramDataContextExpression ? null : type;
        }
    }
}
