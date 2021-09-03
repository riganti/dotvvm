using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{Value}")]
    public class DothtmlLiteralNode : DothtmlNode
    {
        public string Value => Tokens.ConcatTokenText();
        public bool Escape { get; set; } = false;

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return Enumerable.Empty<DothtmlNode>();
        }
        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
