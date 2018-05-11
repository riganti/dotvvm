namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IIdentifierSyntax : IExpressionSyntax
    {
        string Value { get; }
    }
}
