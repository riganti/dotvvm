using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

namespace DotVVM.Framework.Binding
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public abstract class DataContextChangeAttribute : Attribute
    {
        public abstract int Order { get; }

        public abstract ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null);

        public virtual IEnumerable<BindingExtensionParameter> GetExtensionParameters(ITypeDescriptor dataContext) => Enumerable.Empty<BindingExtensionParameter>();

        public virtual IEnumerable<string> PropertyDependsOn => Enumerable.Empty<string>();
    }
}
