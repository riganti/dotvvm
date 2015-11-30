using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlNameNode :DothtmlNode
    {
        public IList<DothtmlToken> WhitespacesBefore { get; set; }
        public DothtmlToken NameToken { get; set; }
        public IList<DothtmlToken> WhitespacesAfter { get; set; }

        public string Text => NameToken.Text;

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return new List<DothtmlNode>();
        }
        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
