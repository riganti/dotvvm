#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PropertyDeclarationBindingParserNode : BindingParserNode
    {
        public BindingParserNode PropertyType { get; private set; }
        public BindingParserNode Name { get; private set; }
        public BindingParserNode? Initializer { get; set; }
        public List<BindingParserNode> Attributes { get; set; } = new List<BindingParserNode>();

        public PropertyDeclarationBindingParserNode(BindingParserNode propertyType, BindingParserNode name)
        {
            PropertyType = propertyType;
            Name = name;
        }

        public override string ToDisplayString() => $"{PropertyType.ToDisplayString()} {Name.ToDisplayString()}{ToInitializerDisplayString()}{ToAttributeSeparatorDisplayString()}{ToAttributeListDisplayString()}";
        private string ToAttributeSeparatorDisplayString() => Attributes.Count != 0 ? "," : "";
        private string ToInitializerDisplayString() => Initializer != null ? $" = {Initializer.ToDisplayString()}" : "";
        private string ToAttributeListDisplayString() => string.Join(", ", Attributes.Select(a => a.ToDisplayString()));

        public override IEnumerable<BindingParserNode> EnumerateNodes()
        {
            var nodes = base.EnumerateNodes().ToList();
            nodes.AddRange(PropertyType.EnumerateNodes());
            nodes.AddRange(Name.EnumerateNodes());
            if (Initializer != null)
            {
                nodes.Add(Initializer);
            }
            nodes.AddRange(Attributes.SelectMany(a => a.EnumerateNodes()));
            return nodes;
        }
        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            var nodes = new List<BindingParserNode> { PropertyType, Name };
            if (Initializer != null)
            {
                nodes.Add(Initializer);
            }
            nodes.AddRange(Attributes);
            return nodes;
        }
    }
}
