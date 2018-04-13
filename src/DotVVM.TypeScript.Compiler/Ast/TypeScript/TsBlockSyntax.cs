using System;
using System.Collections.Generic;
using DotVVM.TypeScript.Compiler.Ast.Visitors;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsBlockSyntax : TsStatementSyntax, IBlockSyntax
    {

        public IList<IStatementSyntax> Statements { get; }

        public TsBlockSyntax(ISyntaxNode parent, IList<IStatementSyntax> statements) : base(parent)
        {
            Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        }
        
        public void AddStatement(IStatementSyntax statement)
        {
            Statements.Add(statement);
        }
        
        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Statements;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitBlockStatement(this);
        }
    }
}
