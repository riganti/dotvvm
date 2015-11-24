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
        public IList<DothtmlToken> WhitespacesBefore { get; set; }
        public IList<DothtmlToken> WhitespacesAfter { get; set; }
    }
}
