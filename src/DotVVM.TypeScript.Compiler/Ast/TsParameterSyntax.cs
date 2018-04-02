using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsParameterSyntax : TsSyntaxNode
    {
        public TsIdentifierSyntax Identifier { get; }
        public TsTypeSyntax Type { get; }
        

        public TsParameterSyntax(TsSyntaxNode parent, TsTypeSyntax type, TsIdentifierSyntax identifier) : base(parent)
        {
            Type = type;
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            return $"{Identifier.ToDisplayString()}: {Type.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(ITsNodeVisitor visitor)
        {
            visitor.VisitParameter(this);
        }
    }
}
