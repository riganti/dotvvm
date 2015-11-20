using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlNodeWithContent : DothtmlNode
    {

        public List<DothtmlNode> Content { get; private set; }

        public DothtmlNodeWithContent()
        {
            Content = new List<DothtmlNode>();
        }


        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(Content.SelectMany(n => n.EnumerateNodes()));
        }

        public override void AddHierarchyByPosition(IList<DothtmlNode> hierarchy, int position)
        {
            hierarchy.Add(this);
            var c = Content.LastOrDefault(n => n.StartPosition <= position);
            c?.AddHierarchyByPosition(hierarchy, position);
        }
    }
}