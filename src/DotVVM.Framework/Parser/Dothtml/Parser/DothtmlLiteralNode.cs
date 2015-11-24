using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{Value}")]
    public class DothtmlLiteralNode : DothtmlNode
    {
        public string Value => string.Join(string.Empty, Tokens.Select(token => token.Text));
        public bool Escape { get; set; } = false;
        public bool IsComment { get; set; }
    }
}