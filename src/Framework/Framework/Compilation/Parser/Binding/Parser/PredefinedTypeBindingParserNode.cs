using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public sealed class PredefinedTypeBindingParserNode : IdentifierNameBindingParserNode
    {
        public PredefinedTypeBindingParserNode(BindingToken nameToken): base(nameToken)
        {
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes() => [];
        public override string ToDisplayString() => Name;
    }
}
