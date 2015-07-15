using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control)
        {
            return ReflectionUtils.GetEnumerableType(dataContext);
        }
    }
}
