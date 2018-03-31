using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsBlockSyntax : TsStatementSyntax
    {

        public IList<TsStatementSyntax> Statements { get; }
        

        public TsBlockSyntax(TsSyntaxNode parent, IList<TsStatementSyntax> statements) : base(parent)
        {
            Statements = statements;
        }

        public void AddStatement(TsStatementSyntax statement)
        {
            Statements.Add(statement);
        }

        public override string ToDisplayString()
        {
            var output = string.Empty;
            foreach (var statement in Statements)
            {
                output += $"{{\n\t{statement.ToDisplayString()};\n}}";
            }
            return output;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Statements;
        }
    }
}
