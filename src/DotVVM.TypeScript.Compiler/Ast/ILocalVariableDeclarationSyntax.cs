using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface ILocalVariableDeclarationSyntax : IStatementSyntax
    {
        IList<IVariableDeclaratorSyntax> Declarators { get; }
    }
}
