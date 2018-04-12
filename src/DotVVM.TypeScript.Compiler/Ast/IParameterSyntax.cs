using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IParameterSyntax : ISyntaxNode
    {
        IIdentifierSyntax Identifier { get; }
        ITypeSyntax Type { get; }
    }
}
