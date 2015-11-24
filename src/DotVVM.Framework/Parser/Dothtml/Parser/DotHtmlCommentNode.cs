using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DotHtmlCommentNode : DothtmlNode
    {
        public string Value => ValeNode.Text;
        public DothtmlToken StartToken { get; set; }
        public DothtmlToken EndToken { get; set; }
        public DothtmlValueTextNode ValeNode { get; set; }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(ValeNode.EnumerateNodes());
        }
    }
}
