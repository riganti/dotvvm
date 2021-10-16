using System;
using System.Linq.Expressions;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticClassIdentifierExpression: Expression
    {
        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Expression Reduce() => throw new Exception($"Can not use type name {this.Type.FullName} as an expression");

        public StaticClassIdentifierExpression(Type type)
            :base()
        {
            this.Type = type;
        }

        public override string ToString() => Type.FullName;
    }
}
