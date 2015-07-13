using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override Type GetChildDataContextType(Type dataContext, Type parentDataContext)
        {
            return ReflectionUtils.GetEnumerableType(dataContext);
        }
    }
}
