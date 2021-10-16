using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class TypeReferenceBindingParserNode : BindingParserNode
    {
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ActualTypeReferenceBindingParserNode : TypeReferenceBindingParserNode
    {
        public readonly BindingParserNode Type;

        public ActualTypeReferenceBindingParserNode(BindingParserNode type)
        {
            this.Type = type;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(Type.EnumerateNodes());

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { Type };

        public override string ToDisplayString()
            => Type.ToDisplayString();
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ArrayTypeReferenceBindingParserNode : TypeReferenceBindingParserNode
    {
        public readonly TypeReferenceBindingParserNode ElementType;

        public ArrayTypeReferenceBindingParserNode(TypeReferenceBindingParserNode elementType)
        {
            this.ElementType = elementType;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(ElementType.EnumerateNodes());

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { ElementType };

        public override string ToDisplayString()
            => $"{ElementType.ToDisplayString()}[]";
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class GenericTypeReferenceBindingParserNode : TypeReferenceBindingParserNode
    {
        public readonly TypeReferenceBindingParserNode Type;
        public readonly List<TypeReferenceBindingParserNode> Arguments;

        public GenericTypeReferenceBindingParserNode(TypeReferenceBindingParserNode type, List<TypeReferenceBindingParserNode> arguments)
        {
            this.Type = type;
            this.Arguments = arguments;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(Type.EnumerateNodes()).Concat(Arguments.SelectMany(arg => arg.EnumerateNodes()));

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { Type }.Concat(Arguments);

        public override string ToDisplayString()
            => $"{Type.ToDisplayString()}<{string.Join(", ", Arguments.Select(e => e.ToDisplayString()))}>";
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class NullableTypeReferenceBindingParserNode : TypeReferenceBindingParserNode
    {
        public readonly TypeReferenceBindingParserNode InnerType;

        public NullableTypeReferenceBindingParserNode(TypeReferenceBindingParserNode innerType)
        {
            this.InnerType = innerType;
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(InnerType.EnumerateNodes());

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { InnerType };

        public override string ToDisplayString()
            => $"{InnerType.ToDisplayString()}?";
    }
}
