using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public class DothtmlDirectiveNode : DothtmlNode
    {

        public string Name { get; set; }

        public string Value { get; set; }
        
    }
}