using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IDoWhileStatementSyntax : IStatementSyntax
    {
        IExpressionSyntax Condition { get; }
        IStatementSyntax Body { get; }
    }
}
