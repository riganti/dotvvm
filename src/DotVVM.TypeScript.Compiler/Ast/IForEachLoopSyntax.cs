using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast.TypeScript;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public interface IForEachLoopSyntax: IStatementSyntax
    {
        IStatementSyntax Body { get; }
        IReferenceSyntax Collection { get; }
        IStatementSyntax Variable { get; }
    }

    public class TsForEachLoopSyntax : TsStatementSyntax, IForEachLoopSyntax
    {
        public TsForEachLoopSyntax(ISyntaxNode parent, IStatementSyntax body, IReferenceSyntax collection, IStatementSyntax variable) : base(parent)
        {
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Collection = collection ?? throw new ArgumentNullException(nameof(collection));
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitForEachLoop(this);
        }

        public IStatementSyntax Body { get; }
        public IReferenceSyntax Collection { get; }
        public IStatementSyntax Variable { get; }
    }
}
