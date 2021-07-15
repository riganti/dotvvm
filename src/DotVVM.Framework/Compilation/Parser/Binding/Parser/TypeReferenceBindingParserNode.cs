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

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            yield return Type;
        }

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

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            yield return ElementType;
        }

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

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            yield return Type;
            foreach (var arg in Arguments)
                yield return arg;
        }

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

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            yield return InnerType;
        }

        public override string ToDisplayString()
            => $"{InnerType.ToDisplayString()}?";
    }
}
