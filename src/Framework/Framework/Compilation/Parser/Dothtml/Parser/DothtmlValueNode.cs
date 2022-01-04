using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public abstract class DothtmlValueNode: DothtmlNode
    {
        public IEnumerable<DothtmlToken> WhitespacesBefore { get; set; } = Enumerable.Empty<DothtmlToken>();
        public IEnumerable<DothtmlToken> WhitespacesAfter { get; set; } = Enumerable.Empty<DothtmlToken>();
        
    }
}
