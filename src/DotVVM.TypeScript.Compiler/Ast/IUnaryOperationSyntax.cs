using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IUnaryOperationSyntax : IExpressionSyntax
    {
        IExpressionSyntax Operand { get; }
        UnaryOperator Operator { get; }
    }
}
