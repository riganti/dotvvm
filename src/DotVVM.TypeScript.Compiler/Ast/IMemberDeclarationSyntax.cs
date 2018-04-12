using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IMemberDeclarationSyntax : ISyntaxNode
    {
        TsModifier Modifier { get; }
        IIdentifierSyntax Identifier { get; set; }
    }
}
