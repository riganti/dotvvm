#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlNameNode :DothtmlNode
    {
        public DothtmlNameNode(DothtmlToken nameToken)
        {
            NameToken = nameToken;
        }

        public IEnumerable<DothtmlToken> WhitespacesBefore { get; set; } = Enumerable.Empty<DothtmlToken>();
        public DothtmlToken NameToken { get; set; }
        public IEnumerable<DothtmlToken> WhitespacesAfter { get; set; } = Enumerable.Empty<DothtmlToken>();

        public string Text => NameToken.Text;

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
