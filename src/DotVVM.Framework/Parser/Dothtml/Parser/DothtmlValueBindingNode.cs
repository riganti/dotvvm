using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlValueBindingNode : DothtmlValueNode
    {
        public DothtmlBindingNode BindingNode { get; set; }

        public List<DothtmlToken> ValueTokens { get; set; } = new List<DothtmlToken>();

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(BindingNode.EnumerateNodes());
        }
    }
}
