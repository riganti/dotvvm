using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IIfStatementSyntax : IStatementSyntax
    {
        IExpressionSyntax ConditionalExpression { get; }
        IStatementSyntax TrueStatement { get; }
        IStatementSyntax FalseStatement { get; }
    }
}
