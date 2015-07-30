using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlLiteralNode : DothtmlNode
    {

        public string Value { get; set; }
        public bool Escape { get; set; } = false;
    }
}