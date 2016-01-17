using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class DothtmlValueNode: DothtmlNode
    {
        public IEnumerable<DothtmlToken> WhitespacesBefore { get; set; }
        public IEnumerable<DothtmlToken> WhitespacesAfter { get; set; }
    }
}
