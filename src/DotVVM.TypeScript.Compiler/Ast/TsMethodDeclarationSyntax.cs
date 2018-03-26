using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsMethodDeclarationSyntax : TsMemberDeclarationSyntax
    {
        public TsBlockSyntax Body { get; }
        public IList<TsParameterSyntax> Parameters { get; }
        public override TsModifier Modifier { get; protected set; }
        public override TsIdentifierSyntax Identifier { get; set; }

        public TsMethodDeclarationSyntax(TsModifier modifier, TsIdentifierSyntax identifier, TsSyntaxNode parent, TsBlockSyntax body, IList<TsParameterSyntax> parameters, TsTypeSyntax type) : base(modifier, identifier, parent)
        {
            Body = body;
            Parameters = parameters;
        }
        
        public override string ToDisplayString()
        {
            return $"{Modifier.ToDisplayString()} {Identifier.ToDisplayString()}({Parameters.Select(p => p.ToDisplayString()).StringJoin(",")})" +
                   "{\n" +
                   $"{Body.ToDisplayString()}\n" +
                   "}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            yield return Body;
        }
    }
}
