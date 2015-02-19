using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Parser.RwHtml.Tokenizer;

namespace Redwood.Framework.Parser.RwHtml.Parser
{
    public abstract class RwHtmlNode
    {

        public int StartPosition
        {
            get { return Tokens.First().StartPosition; }
        }

        public int Length
        {
            get { return Tokens.Sum(t => t.Length); }
        }

        public List<RwHtmlToken> Tokens { get; private set; }
        
        public RwHtmlNode()
        {
            Tokens = new List<RwHtmlToken>();
        }

    }
}