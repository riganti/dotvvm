using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Binding
{
    public class CollectionElementDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override Type GetChildDataContextType(Type dataContext, Type parentDataContext, RedwoodControl control)
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
