#nullable enable
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
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
