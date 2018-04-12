namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsReferenceSyntax : TsExpressionSyntax, IReferenceSyntax
    {
        public IIdentifierSyntax Identifier { get; protected set; }

        protected TsReferenceSyntax(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
