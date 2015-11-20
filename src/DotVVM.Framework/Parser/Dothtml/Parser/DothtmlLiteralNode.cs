using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{Value}")]
    public class DothtmlLiteralNode : DothtmlNode
    {
        public string Value { get; set; }
        public bool Escape { get; set; } = false;
        public bool IsComment { get; set; }

        public override void AddHierarchyByPosition(IList<DothtmlNode> hierarchy, int position)
        {
            hierarchy.Add(this);
        }
    }
}