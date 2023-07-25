using System;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.Binding
{
    public sealed class UnknownStaticClassIdentifierExpression: Expression
    {
        public UnknownStaticClassIdentifierExpression(string name, BindingParserNode? node = null)
        {
            Name = name;
            Node = node;
        }

        public string Name { get; }
        public BindingParserNode? Node { get; internal set; }

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
        {
            var message = $"Could not resolve identifier '{Name}'.";
            return Node is null ? new Exception(message) : new BindingCompilationException(message, Node);
        }

        public override string ToString() => $"[unknown identifier {Name}]";
    }
}
