using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FormattedBindingParserNode : BindingParserNode
    {
        public BindingParserNode Node { get; private set; }
        public string Format { get; private set; }

        public FormattedBindingParserNode(BindingParserNode node, string format)
        {
            this.Node = node;
            this.Format = format;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(Node.EnumerateNodes());

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { Node };

        public override string ToDisplayString()
            => $"{Node.ToDisplayString()}:{Format}";
    }
}
