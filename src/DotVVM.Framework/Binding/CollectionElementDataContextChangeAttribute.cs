using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime.Compilation;
using System.Linq.Expressions;
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

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control, DotvvmProperty property = null)
        {
            return ReflectionUtils.GetEnumerableType(dataContext);
        }
    }
}
