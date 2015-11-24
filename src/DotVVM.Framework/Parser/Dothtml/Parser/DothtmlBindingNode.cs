using DotVVM.Framework.Parser.Dothtml.Tokenizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    [DebuggerDisplay("{debuggerDisplay,nq}")]
    public class DothtmlBindingNode : DothtmlNode
    {

        #region debbuger display
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

        public override IEnumerable<DothtmlNode> EnumerateNodes()
        {
            var enumeration = base.EnumerateNodes().Concat(NameNode.EnumerateNodes() );
            if(ValueNode != null )
            {
                enumeration = enumeration.Concat(ValueNode.EnumerateNodes());
            }
            return enumeration;
        }
    }
}