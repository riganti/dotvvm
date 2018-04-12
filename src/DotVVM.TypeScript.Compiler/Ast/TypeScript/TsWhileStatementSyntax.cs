using System;
using System.Collections.Generic;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsWhileStatementSyntax : TsStatementSyntax, IWhileStatementSyntax
    {
        public IExpressionSyntax Condition { get; }
        public IStatementSyntax Body { get; }

        public TsWhileStatementSyntax(ISyntaxNode parent, IExpressionSyntax condition, IStatementSyntax body) :
            base(parent)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public override string ToDisplayString()
        {
            return $"while({Condition.ToDisplayString()})\n" +
                   $"\t{Body.ToDisplayString()}";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            yield return Body;
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }
    }
}
