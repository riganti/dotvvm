using System;
using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsSyntaxNode : ISyntaxNode
    {
        protected TsSyntaxNode(ISyntaxNode parent)
        {
            this.Parent = parent;
        }

        public ISyntaxNode Parent { get; }

        public string ToDisplayString()
        {
            var visitor = new TsFormattingVisitor();
            AcceptVisitor(visitor);
            return visitor.GetOutput();
        }

        public abstract IEnumerable<ISyntaxNode> DescendantNodes();
        public abstract void AcceptVisitor(INodeVisitor visitor);

        public override string ToString()
        {
            return ToDisplayString();
        }
    }
}
