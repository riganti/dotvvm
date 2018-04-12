namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsStatementSyntax : TsSyntaxNode, IStatementSyntax
    {
        public TsStatementSyntax(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
