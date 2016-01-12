using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime.Compilation;
using System.Linq.Expressions;
using DotVVM.Framework.Runtime.ControlTree;
using DotVVM.Framework.Runtime.ControlTree.Resolved;

namespace DotVVM.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override int Order { get; }

        public CollectionElementDataContextChangeAttribute(int order)
        {
            Order = order;
        }
        
        public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
        {
            return TypeDescriptorUtils.GetCollectionItemType(dataContext);
        }
    }
}
