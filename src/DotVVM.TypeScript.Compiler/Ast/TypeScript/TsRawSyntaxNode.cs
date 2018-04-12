using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    class TsRawSyntaxNode : TsExpressionSyntax, IRawSyntaxNode
    {
        public string Value { get;  }

        public TsRawSyntaxNode(ISyntaxNode parent, string value) : base(parent)
        {
            Value = value;
        }

        public override string ToDisplayString()
        {
            return Value;
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<ISyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitParametrizedSyntaxNode(this);
        }
    }
}
