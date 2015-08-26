using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class StaticClassIdentifierExpression: Expression
    {
        private Type type;
        public override Type Type
        {
            get
            {
                return type;
            }
        }

        public StaticClassIdentifierExpression(Type type)
            :base()
        {
            this.type = type;
        }
    }
}
