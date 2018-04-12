using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsParameterSyntax : TsSyntaxNode, IParameterSyntax
    {
        public IIdentifierSyntax Identifier { get; }
        public ITypeSyntax Type { get; }
        

        public TsParameterSyntax(ISyntaxNode parent, ITypeSyntax type, IIdentifierSyntax identifier) : base(parent)
        {
            Type = type;
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            return $"{Identifier.ToDisplayString()}: {Type.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitParameter(this);
        }
    }
}
