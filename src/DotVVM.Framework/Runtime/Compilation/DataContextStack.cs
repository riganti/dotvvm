using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;

namespace DotVVM.Framework.Runtime.Compilation
{
    public class DataContextStack : IDataContextStack
    {
        public DataContextStack Parent { get; set; }
        public Type DataContextType { get; set; }
        public Type RootControlType { get; set; }

        public DataContextStack(Type type, DataContextStack parent = null)
        {
            Parent = parent;
            DataContextType = type;
            RootControlType = parent?.RootControlType;
        }
        
        public IEnumerable<Type> Enumerable()
        {
            var c = this;
            while (c != null)
            {
                yield return c.DataContextType;
                c = c.Parent;
            }
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

        ITypeDescriptor IDataContextStack.DataContextType => new ResolvedTypeDescriptor(DataContextType);
        IDataContextStack IDataContextStack.Parent => Parent;
    }
}
