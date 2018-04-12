using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IParenthesizedExpressionSyntax : IExpressionSyntax
    {
        IExpressionSyntax Expression { get; }
    }
}
