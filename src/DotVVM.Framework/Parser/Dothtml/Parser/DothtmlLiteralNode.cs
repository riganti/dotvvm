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
    }
}