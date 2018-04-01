using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public abstract class TsSyntaxNode
    {
        protected TsSyntaxNode(TsSyntaxNode parent)
        {
            this.Parent = parent;
        }

        public TsSyntaxNode Parent { get; }

        public abstract string ToDisplayString();
        public abstract IEnumerable<TsSyntaxNode> DescendantNodes();

        public override string ToString()
        {
            return ToDisplayString();
        }
    }
}
