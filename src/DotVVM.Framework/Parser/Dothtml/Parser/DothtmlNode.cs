using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Parser.Dothtml.Parser
{
    public abstract class DothtmlNode
    {
        public int StartPosition => Tokens.First().StartPosition;

        public int Length => Tokens.Select(t => t.Length).Sum();

        public AggregateList<DothtmlToken> Tokens { get; private set; } = new AggregateList<DothtmlToken>();

        public DothtmlNode ParentNode { get; set; }

        private List<string> nodeErrors;
        public IEnumerable<string> NodeErrors => nodeErrors ?? Enumerable.Empty<string>();
        public void AddError(string error)
        {
            if (nodeErrors == null) nodeErrors = new List<string>();
            nodeErrors.Add(error);
        }
        public bool HasNodeErrors
        {
            get { return nodeErrors != null && nodeErrors.Count > 0; }
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