#nullable enable
using DotVVM.Framework.Compilation.Parser.Binding.Tokenizer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    
    public sealed class VoidBindingParserNode : BindingParserNode
    {

        public VoidBindingParserNode()
        {
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes() => Enumerable.Empty<BindingParserNode>();
        public override string ToDisplayString() => "";
    }
}
