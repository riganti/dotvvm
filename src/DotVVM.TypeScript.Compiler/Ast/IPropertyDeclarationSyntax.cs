using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IPropertyDeclarationSyntax : IMemberDeclarationSyntax
    {
        ITypeSyntax Type { get; }
    }
}
