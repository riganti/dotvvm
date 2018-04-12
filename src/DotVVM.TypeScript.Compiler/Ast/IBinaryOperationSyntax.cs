using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IBinaryOperationSyntax : IExpressionSyntax
    {
        IExpressionSyntax LeftExpression { get; }
        BinaryOperator Operator { get; }
        IExpressionSyntax RightExpression { get; }
    }
}
