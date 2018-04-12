using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface ISyntaxNode
    {
        ISyntaxNode Parent { get; }
        string ToDisplayString();
        IEnumerable<ISyntaxNode> DescendantNodes();
        void AcceptVisitor(INodeVisitor visitor);
    }
}
