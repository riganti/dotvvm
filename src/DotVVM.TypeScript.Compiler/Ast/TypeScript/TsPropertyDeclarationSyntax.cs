﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsPropertyDeclarationSyntax : TsMemberDeclarationSyntax, IPropertyDeclarationSyntax
    {
        public ITypeSyntax Type { get; }

        public override AccessModifier Modifier { get; protected set; }
        public override IIdentifierSyntax Identifier { get; protected set; }

        public TsPropertyDeclarationSyntax(AccessModifier modifier, IIdentifierSyntax identifier, ISyntaxNode parent,
            ITypeSyntax type) : base(modifier, identifier, parent)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override string ToDisplayString()
        {
            return $"{Modifier.ToDisplayString()} {Identifier.ToDisplayString()}: {Type.ToDisplayString()};";
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitPropertyDeclaration(this);
        }
    }
}
