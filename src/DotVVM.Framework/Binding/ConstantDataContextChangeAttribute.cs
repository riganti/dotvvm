using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;

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
        
        public override ITypeDescriptor GetChildDataContextType(ITypeDescriptor dataContext, IDataContextStack controlContextStack, IAbstractControl control, IPropertyDescriptor property = null)
        {
            return new ResolvedTypeDescriptor(type);
        }
    }
}
