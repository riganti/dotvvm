using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IBlockSyntax : IStatementSyntax
    {
        IList<IStatementSyntax> Statements { get; }
        void AddStatement(IStatementSyntax statement);
    }
}
