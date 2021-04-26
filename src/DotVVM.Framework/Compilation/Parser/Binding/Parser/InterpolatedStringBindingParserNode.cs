using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterpolatedStringBindingParserNode : BindingParserNode
    {
        public string Format { get; set; }
        public List<BindingParserNode> Arguments { get; set; }

        public InterpolatedStringBindingParserNode(string format, List<BindingParserNode> arguments)
        {
            this.Format = format;
            this.Arguments = arguments;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => base.EnumerateNodes().Concat(Arguments);

        public override string ToDisplayString()
            => $"String.Format(\"{Format}\", {Arguments.Select(arg => arg.ToDisplayString()).StringJoin(", ")})";
    }
}
