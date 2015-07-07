using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlRootNode : DothtmlNodeWithContent
    {
        
        public List<DothtmlDirectiveNode> Directives { get; private set; }



        public DothtmlRootNode()
        {
            Directives = new List<DothtmlDirectiveNode>();
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Directives.SelectMany(d => d.EnumerateNodes()));
        }
        
    }
}
