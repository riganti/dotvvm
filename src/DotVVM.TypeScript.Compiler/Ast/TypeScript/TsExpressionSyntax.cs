namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsExpressionSyntax : TsStatementSyntax, IExpressionSyntax
    {
        protected TsExpressionSyntax(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
