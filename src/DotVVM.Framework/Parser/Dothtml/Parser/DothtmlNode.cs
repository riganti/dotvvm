using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class DothtmlNode
    {
        public int StartPosition { get; internal set; }

        public int Length => Tokens.Select(t => t.Length).DefaultIfEmpty(0).Sum();

        public List<DothtmlToken> Tokens { get; private set; } = new List<DothtmlToken>();

        public DothtmlNode ParentNode { get; set; }

        public List<string> NodeErrors { get; private set; } = new List<string>();

        public bool HasNodeErrors
        {
            get { return NodeErrors.Any(); }
        }

        public abstract void Accept(IDothtmlSyntaxTreeVisitor visitor);

        public abstract IEnumerable<DothtmlNode> EnumerateChildNodes();

        public DothtmlNode FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.StartPosition + n.Length);
        }

        public virtual IEnumerable<DothtmlNode> EnumerateNodes()
        {
            yield return this;
        }
    }
}