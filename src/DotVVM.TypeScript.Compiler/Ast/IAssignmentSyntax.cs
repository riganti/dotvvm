using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IAssignmentSyntax : IStatementSyntax
    {
        IReferenceSyntax Reference { get; }
        IExpressionSyntax Expression { get; }
    }
}
