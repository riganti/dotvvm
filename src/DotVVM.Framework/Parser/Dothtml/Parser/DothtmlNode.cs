using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class DothtmlNode
    {
        public int StartPosition { get; internal set; }

        public int Length { get; internal set; }

        public List<DothtmlToken> Tokens { get; private set; }

        public DothtmlNode ParentNode { get; set; }

        public List<string> NodeErrors { get; private set; }

        public bool HasNodeErrors
        {
            get { return NodeErrors.Any(); }
        }

        public DothtmlNode()
        {
            Tokens = new List<DothtmlToken>();
            NodeErrors = new List<string>();
        }


        public virtual IEnumerable<DothtmlNode> EnumerateNodes()
        {
            yield return this;
        }

        public DothtmlNode FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.StartPosition + n.Length);
        }

        public IList<DothtmlNode> FindHierarchyByPosition(int position)
        {
            var list = new List<DothtmlNode>();
            AddHierarchyByPosition(list, position);
            return list;
        }

        public abstract void AddHierarchyByPosition(IList<DothtmlNode> hierarchy, int position);
    }
}