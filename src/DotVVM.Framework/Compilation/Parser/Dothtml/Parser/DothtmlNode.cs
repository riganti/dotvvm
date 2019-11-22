#nullable enable
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Dothtml.Parser
{
    public abstract class DothtmlNode : ITextRange
    {
        public int StartPosition => Tokens.FirstOrDefault()?.StartPosition ?? 0;

        public int Length => Tokens.SumTokenLength();
        public int EndPosition => StartPosition + Length; 

        public AggregateList<DothtmlToken> Tokens { get; private set; } = new AggregateList<DothtmlToken>();

        public DothtmlNode? ParentNode { get; set; }

        private List<string>? nodeWarnings;
        private List<string>? nodeErrors;

        public IEnumerable<string> NodeWarnings => nodeWarnings ?? Enumerable.Empty<string>();
        public IEnumerable<string> NodeErrors => nodeErrors ?? Enumerable.Empty<string>();

        public void AddError(string error)
        {
            if (nodeErrors == null) nodeErrors = new List<string>();
            nodeErrors.Add(error);
        }
        public void AddWarning(string error)
        {
            if(nodeWarnings == null) nodeWarnings = new List<string>();
            nodeWarnings.Add(error);
        }


        public bool HasNodeErrors
        {
            get { return nodeErrors != null && nodeErrors.Count > 0; }
        }

        public abstract void Accept(IDothtmlSyntaxTreeVisitor visitor);

        public abstract IEnumerable<DothtmlNode> EnumerateChildNodes();

        public DothtmlNode FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.EndPosition);
        }

        public virtual IEnumerable<DothtmlNode> EnumerateNodes()
        {
            yield return this;
        }
    }
}
