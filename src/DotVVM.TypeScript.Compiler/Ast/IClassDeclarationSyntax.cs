using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IClassDeclarationSyntax : IMemberDeclarationSyntax
    {
        IList<IMemberDeclarationSyntax> Members { get; }
        IList<IIdentifierSyntax> BaseClasses { get; }
    }
}
