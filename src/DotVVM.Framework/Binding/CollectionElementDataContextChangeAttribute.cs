using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override Type GetChildDataContextType(Type dataContext, Type parentDataContext, DotvvmControl control)
        {
            if(dataContext.IsArray)
            {
                return dataContext.GetElementType();
            }
            var ienumerable = dataContext.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (ienumerable != null)
            {
                return ienumerable.GetGenericArguments()[0];
            }
            else throw new NotSupportedException();
        }
    }
}
