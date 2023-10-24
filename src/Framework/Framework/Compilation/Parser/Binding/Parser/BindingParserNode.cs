using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Binding.Parser.Annotations;
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class BindingParserNode : ITextRange
    {
        protected string DebuggerDisplay => $"(S: {StartPosition}, L: {Length}, Err: {NodeErrors.Count}): {ToDisplayString()}";

        public int StartPosition { get; internal set; }
        public int Length { get; internal set; }
        public int EndPosition => StartPosition + Length;

        public List<BindingToken> Tokens { get; internal set; }
        public HashSet<IBindingParserAnnotation> Annotations { get; private set; }

        public List<string> NodeErrors { get; private set; }

        public bool HasNodeErrors
        {
            get { return NodeErrors.Any(); }
        }

        public BindingParserNode()
        {
            Tokens = new List<BindingToken>();
            NodeErrors = new List<string>();
            Annotations = new HashSet<IBindingParserAnnotation>();
        }


        public virtual IEnumerable<BindingParserNode> EnumerateNodes()
        {
            yield return this;
        }

        public BindingParserNode? FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.EndPosition);
        }

        public abstract IEnumerable<BindingParserNode> EnumerateChildNodes();

        public abstract string ToDisplayString();

        internal void TransferTokens(BindingParserNode sourceNode, int? startPosition = null)
        {
            
            StartPosition = startPosition ?? sourceNode.StartPosition;
            Length = 0;

            Tokens.Clear();
            Tokens.Capacity = sourceNode.Tokens.Capacity;

            for (int i = 0; i < sourceNode.Tokens.Count; i++)
            {
                var token = sourceNode.Tokens[i];
                if ((startPosition ?? 0) <= token.StartPosition)
                {
                    Length += token.Length;
                    Tokens.Add(token);
                }
            }
        }
    }
}
