#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public class DothtmlValueTextNode : DothtmlValueNode
    {
        public DothtmlValueTextNode(DothtmlToken valueToken)
        {
            ValueToken = valueToken;
        }

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
