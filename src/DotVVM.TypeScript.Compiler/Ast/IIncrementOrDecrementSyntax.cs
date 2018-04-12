using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IIncrementOrDecrementSyntax : IExpressionSyntax
    {
        IExpressionSyntax Target { get; }
        bool IsPostfix { get; }
        bool IsIncrement { get; }
    }
}
