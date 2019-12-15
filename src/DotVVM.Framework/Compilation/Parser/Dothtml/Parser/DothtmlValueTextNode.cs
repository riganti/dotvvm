using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlValueTextNode : DothtmlValueNode
    {
        public Tokenizer.DothtmlToken ValueToken { get; set; }
        public string Text => ValueToken.Text;

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            return Enumerable.Empty<DothtmlNode>();
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
