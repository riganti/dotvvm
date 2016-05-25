using System;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Binding
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
