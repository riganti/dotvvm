using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{Value}")]
    public sealed class DothtmlLiteralNode : DothtmlNode
    {
        public DothtmlToken? MainValueToken { get; set; }
        public string Value => MainValueToken is {} ? MainValueToken.Text : Tokens.ConcatTokenText();
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
