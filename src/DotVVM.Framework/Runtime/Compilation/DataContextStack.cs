using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class DataContextStack
    {
        public DataContextStack Parent { get; set; }
        public Type DataContextType { get; set; }

        public DataContextStack(Type type, DataContextStack parent = null)
        {
            Parent = parent;
            DataContextType = type;
        }

        public IEnumerable<Type> Parents()
        {
            var c = Parent;
            while(c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
        }
    }
}
