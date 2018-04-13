using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsMethodCallSyntax : TsExpressionSyntax, IMethodCallSyntax
    {
        public IReferenceSyntax Object { get; set; }
        public IIdentifierSyntax Name { get; }
        public ImmutableList<IExpressionSyntax> Arguments { get; private set; }
        public void SetArguments(ImmutableList<IExpressionSyntax> arguments)
        {
            Arguments = arguments;
        }

        public TsMethodCallSyntax(ISyntaxNode parent, IIdentifierSyntax name,
            ImmutableList<IExpressionSyntax> parameters, IReferenceSyntax @object) : base(parent)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Object = @object;
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitMethodCall(this);
        }
    }
}
