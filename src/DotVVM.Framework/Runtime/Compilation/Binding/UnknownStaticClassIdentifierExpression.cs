using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class UnknownStaticClassIdentifierExpression: Expression
    {
        public UnknownStaticClassIdentifierExpression(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override Type Type
        {
            get
            {
                throw Error();
            }
        }

        public override Expression Reduce()
        {
            throw Error();
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            throw Error();
        }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            throw Error();
        }

        public override ExpressionType NodeType
        {
            get
            {
                throw Error();
            }
        }

        public Exception Error()
            => new Exception($"Could not resolve identifier '{Name}'.");
    }
}
