using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsMethodDeclarationSyntax : TsMemberDeclarationSyntax, IMethodDeclarationSyntax
    {
        public IBlockSyntax Body { get; }
        public IList<IParameterSyntax> Parameters { get; }
        public override TsModifier Modifier { get; protected set; }
        public override IIdentifierSyntax Identifier { get; set; }

        public TsMethodDeclarationSyntax(TsModifier modifier, IIdentifierSyntax identifier, ISyntaxNode parent, IBlockSyntax body, IList<IParameterSyntax> parameters, ITypeSyntax type) : base(modifier, identifier, parent)
        {
            Body = body;
            Parameters = parameters;
        }
        
        public override string ToDisplayString()
        {
            return $"{Modifier.ToDisplayString()} {Identifier.ToDisplayString()}({Parameters.Select(p => p.ToDisplayString()).StringJoin(",")})" +
                    $"\t{Body.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitMethodDeclaration(this);
        }
    }
}
