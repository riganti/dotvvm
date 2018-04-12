using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IWhileStatementSyntax : IStatementSyntax
    {
        IExpressionSyntax Condition { get; }
        IStatementSyntax Body { get; }
    }
}
