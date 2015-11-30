using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class DothtmlNodeWithContent : DothtmlNode
    {
        public List<DothtmlNode> Content { get; private set; } = new List<DothtmlNode>();

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return Content;
        }
    }
}