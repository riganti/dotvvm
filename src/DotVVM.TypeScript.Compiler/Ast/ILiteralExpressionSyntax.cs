namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface ILiteralExpressionSyntax : IExpressionSyntax
    {
        string Value { get; }
    }
}