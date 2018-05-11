using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    class TsObjectCreationExpressionSyntax : TsExpressionSyntax, IObjectCreationExpressionSyntax
    {
        public ITypeSyntax ObjectType { get; }

        public IList<IExpressionSyntax> Arguments { get; }

        public TsObjectCreationExpressionSyntax(ISyntaxNode parent, IList<IExpressionSyntax> argumentsSyntax, ITypeSyntax objectType) : base(parent)
        {
            Arguments = argumentsSyntax;
            ObjectType = objectType;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitObjectCreationExpresion(this);
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<ISyntaxNode>();
        }


    }
}
