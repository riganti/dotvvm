using System;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Binding
{
    public class UnknownStaticClassIdentifierExpression: Expression
    {
        public UnknownStaticClassIdentifierExpression(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public override Type Type => throw Error();

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

        public override ExpressionType NodeType => throw Error();

        public Exception Error()
            => new Exception($"Could not resolve identifier '{Name}'.");
    }
}
