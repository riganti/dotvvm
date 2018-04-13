using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsMethodDeclarationSyntax : TsMemberDeclarationSyntax, IMethodDeclarationSyntax
    {
        public IBlockSyntax Body { get; }
        public IList<IParameterSyntax> Parameters { get; }
        public override AccessModifier Modifier { get; protected set; }
        public override IIdentifierSyntax Identifier { get; protected set; }

        public TsMethodDeclarationSyntax(AccessModifier modifier, IIdentifierSyntax identifier, ISyntaxNode parent,
            IBlockSyntax body, IList<IParameterSyntax> parameters) : base(modifier, identifier, parent)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            Modifier = modifier;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitMethodDeclaration(this);
        }
    }
}
