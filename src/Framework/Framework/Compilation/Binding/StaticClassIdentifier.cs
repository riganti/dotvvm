using System;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Binding
{
    public sealed class StaticClassIdentifierExpression: Expression
    {
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public Exception Error() => new Exception($"Cannot use type name {this.Type.FullName} as an expression");
        public override Expression Reduce() => throw Error();
        protected override Expression VisitChildren(ExpressionVisitor visitor) => throw Error();
        protected override Expression Accept(ExpressionVisitor visitor) => throw Error();

        public StaticClassIdentifierExpression(Type type)
            :base()
        {
            this.Type = type;
        }

        public override string ToString() => Type.FullName ?? "";
    }
}
