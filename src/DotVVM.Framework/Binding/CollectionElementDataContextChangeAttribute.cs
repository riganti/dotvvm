#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override int Order { get; }

        public CollectionElementDataContextChangeAttribute(int order)
        {
            Order = order;
        }
        
        public override ITypeDescriptor? GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor? property = null)
        {
            return TypeDescriptorUtils.GetCollectionItemType(dataContext);
        }

        public override Type? GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, DotvvmBindableObject control, DotvvmProperty? property = null)
        {
            return ReflectionUtils.GetEnumerableType(dataContext);
        }

        public override IEnumerable<BindingExtensionParameter> GetExtensionParameters(ITypeDescriptor dataContext)
        {
            return base.GetExtensionParameters(dataContext).Concat(new BindingExtensionParameter[] { new CurrentCollectionIndexExtensionParameter(), new BindingCollectionInfoExtensionParameter("_collection") });
        }
    }
}
