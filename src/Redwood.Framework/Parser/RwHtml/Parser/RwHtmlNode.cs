using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public abstract class RwHtmlNode
    {

        public int StartPosition { get; internal set; }

        public int Length { get; internal set; }
         

        public List<RwHtmlToken> Tokens { get; private set; }


        public List<string> NodeErrors { get; private set; }

        public bool HasNodeErrors
        {
            get { return NodeErrors.Any(); }
        }

        public RwHtmlNode()
        {
            Tokens = new List<RwHtmlToken>();
            NodeErrors = new List<string>();
        }


        public virtual IEnumerable<RwHtmlNode> EnumerateNodes()
        {
            yield return this;
        }



        public RwHtmlNode FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.StartPosition + n.Length);
        }
    }
}