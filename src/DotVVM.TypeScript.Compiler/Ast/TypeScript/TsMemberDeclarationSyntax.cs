using System;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public abstract class TsMemberDeclarationSyntax : TsSyntaxNode, IMemberDeclarationSyntax
    {
        public abstract AccessModifier Modifier { get; protected set; }
        public abstract IIdentifierSyntax Identifier { get; protected set; }
        
        protected TsMemberDeclarationSyntax(AccessModifier modifier, IIdentifierSyntax identifier, ISyntaxNode parent) : base(parent)
        {
            Modifier = modifier;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
    }
}
