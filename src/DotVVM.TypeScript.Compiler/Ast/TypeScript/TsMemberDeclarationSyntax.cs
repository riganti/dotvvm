namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsMemberDeclarationSyntax : TsSyntaxNode, IMemberDeclarationSyntax
    {
        public abstract TsModifier Modifier { get; protected set; }
        public abstract IIdentifierSyntax Identifier { get; set; }
        protected TsMemberDeclarationSyntax(TsModifier modifier, IIdentifierSyntax identifier, ISyntaxNode parent) : base(parent)
        {
            Modifier = modifier;
            Identifier = identifier;
        }
    }
}
