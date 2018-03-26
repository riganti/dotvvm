using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsBlockSyntax : TsSyntaxNode
    {

        public IList<TsStatementSyntax> Statements { get; }
        

        public TsBlockSyntax(TsSyntaxNode parent, IList<TsStatementSyntax> statements) : base(parent)
        {
            Statements = statements;
        }

        public override string ToDisplayString()
        {
            return $"\t{Statements.Select(s => s.ToDisplayString()).StringJoin(";\n")}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Statements;
        }
    }
}
