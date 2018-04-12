using DotVVM.TypeScript.Compiler.Ast.TypeScript;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IConditionalExpressionSyntax : IExpressionSyntax
    {
         IExpressionSyntax Condition { get; }
         IExpressionSyntax WhenTrue { get; }
         IExpressionSyntax WhenFalse { get; }
    }
}
