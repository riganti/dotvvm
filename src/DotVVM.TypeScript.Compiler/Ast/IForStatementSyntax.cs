using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IForStatementSyntax : IStatementSyntax
    {
        IStatementSyntax BeforeStatement { get; }
        IExpressionSyntax Condition { get; }
        IStatementSyntax AfterExpression { get; }
        IStatementSyntax Body { get; }
    }
}
