using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public class AssemblyQualifiedNameBindingParserNode: BindingParserNode
    {
        public BindingParserNode TypeName { get; }
        public BindingParserNode AssemblyName { get; }

        public AssemblyQualifiedNameBindingParserNode(BindingParserNode typeName, BindingParserNode assemblyName)
        {
            this.TypeName = typeName;
            this.AssemblyName = assemblyName;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes() => new[] { TypeName, AssemblyName };

        public override string ToDisplayString() => $"{TypeName.ToDisplayString()}, {AssemblyName.ToDisplayString()}";
    }
}
