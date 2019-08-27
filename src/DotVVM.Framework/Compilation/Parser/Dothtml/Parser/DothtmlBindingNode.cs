using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class DothtmlBindingNode : DothtmlNode
    {

        #region debugger display
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string debuggerDisplay
        {
            get
            {
                return "{" + Name + ": " + Value + "}";
            }
        }
        #endregion


        public DothtmlToken StartToken { get; set; }
        public DothtmlToken EndToken { get; set; }
        public DothtmlToken SeparatorToken { get; set; }

        public DothtmlNameNode NameNode { get; set; }
        public DothtmlValueTextNode ValueNode { get; set; }

        public string Name => NameNode.Text;

        public string Value => ValueNode.Text;

        public override IEnumerable<DothtmlNode> EnumerateChildNodes()
        {
            if (NameNode != null)
            {
                yield return NameNode;
            }
            if (ValueNode != null)
            {
                yield return ValueNode;
            }
        }

        public override void Accept(IDothtmlSyntaxTreeVisitor visitor)
        {
            visitor.Visit(this);

            foreach (var node in EnumerateChildNodes())
            {
                if (visitor.Condition(node))
                {
                    node.Accept(visitor);
                }
            }
        }

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            return base.EnumerateNodes().Concat(EnumerateChildNodes().SelectMany(node => node.EnumerateNodes()));
        }
    }
}
