using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IMethodDeclarationSyntax : IMemberDeclarationSyntax
    {
        IBlockSyntax Body { get; }
        IList<IParameterSyntax> Parameters { get; }
    }
}
