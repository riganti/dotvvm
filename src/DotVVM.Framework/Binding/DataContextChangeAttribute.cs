using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using DotVVM.Framework.Runtime.Compilation;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute : Attribute
    {
        public abstract int Order { get; }

        public abstract ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null);
        
        public static ITypeDescriptor GetDataContextExpression(IDataContextStack dataContextStack, ResolvedControl control, DotvvmProperty property = null)
        {
            var attributes = property == null ? control.Metadata.Type.GetCustomAttributes<DataContextChangeAttribute>().ToArray() : property.PropertyInfo?.GetCustomAttributes<DataContextChangeAttribute>().ToArray();
            if (attributes == null || attributes.Length == 0) return null;

            var type = dataContextStack.DataContextType;
            foreach (var attribute in attributes.OrderBy(a => a.Order))
            {
                type = attribute.GetChildDataContextType(type, dataContextStack, control, property);
            }
            return type;
        }
    }
}
