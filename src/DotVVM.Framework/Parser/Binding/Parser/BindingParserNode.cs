using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Parser.Binding.Parser
{
    public abstract class BindingParserNode
    {

        public int StartPosition { get; internal set; }

        public int Length { get; internal set; }


        public List<BindingToken> Tokens { get; internal set; }


        public List<string> NodeErrors { get; private set; }

        public bool HasNodeErrors
        {
            get { return NodeErrors.Any(); }
        }

        public BindingParserNode()
        {
            Tokens = new List<BindingToken>();
            NodeErrors = new List<string>();
        }


        public virtual IEnumerable<BindingParserNode> EnumerateNodes()
        {
            yield return this;
        }

        public BindingParserNode FindNodeByPosition(int position)
        {
            return EnumerateNodes().LastOrDefault(n => n.StartPosition <= position && position < n.StartPosition + n.Length);
        }

    }
}