using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Compilation;
using DotVVM.Framework.Runtime.Compilation.ResolvedControlTree;

namespace DotVVM.Framework.Binding
{
    public class ConstantDataContextChangeAttribute : DataContextChangeAttribute
    {
        public override int Order { get; }
        protected Type type;

        public ConstantDataContextChangeAttribute(Type type, int order = 0)
        {
            this.type = type;
            Order = order;
        }

        public override Type GetChildDataContextType(Type dataContext, DataContextStack controlContextStack, ResolvedControl control, DotvvmProperty property = null)
        {
            return type;
        }
    }
}
