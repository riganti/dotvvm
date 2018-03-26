namespace DotVVM.TypeScript.Compiler.Ast
{
    public abstract class TsMemberDeclarationSyntax : TsSyntaxNode
    {
        public abstract TsModifier Modifier { get; protected set; }
        public abstract TsIdentifierSyntax Identifier { get; set; }

        protected TsMemberDeclarationSyntax(TsModifier modifier, TsIdentifierSyntax identifier, TsSyntaxNode parent) : base(parent)
        {
            Modifier = modifier;
            Identifier = identifier;
        }
    }
}
