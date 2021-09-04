using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class TypeOrFunctionReferenceBindingParserNode : BindingParserNode
    {
        public readonly BindingParserNode TypeOrFunction;
        public readonly List<TypeReferenceBindingParserNode> TypeArguments;

        public TypeOrFunctionReferenceBindingParserNode(BindingParserNode typeOrFunction, List<TypeReferenceBindingParserNode>? typeArguments = null)
        {
            this.TypeOrFunction = typeOrFunction;
            this.TypeArguments = typeArguments ?? new List<TypeReferenceBindingParserNode>(0);
        }

        public override IEnumerable<BindingParserNode> EnumerateNodes()
            => base.EnumerateNodes().Concat(TypeOrFunction.EnumerateNodes()).Concat(TypeArguments.SelectMany(arg => arg.EnumerateNodes()));

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
            => new[] { TypeOrFunction }.Concat(TypeArguments);

        public override string ToDisplayString()
        {
            var suffix = (TypeArguments.Count == 0) ? string.Empty : $"<{string.Join(", ", TypeArguments.Select(e => e.ToDisplayString()))}>";
            return $"{TypeOrFunction.ToDisplayString()}{suffix}";
        }

        public TypeReferenceBindingParserNode ToTypeReference()
        {
            var type = new ActualTypeReferenceBindingParserNode(TypeOrFunction);
            if (TypeArguments.Count == 0)
                return type;

            return new GenericTypeReferenceBindingParserNode(type, TypeArguments);
        }

        public BindingParserNode ToFunctionReference()
        {
            if (TypeOrFunction is SimpleNameBindingParserNode simpleName)
            {
                if (TypeArguments.Count == 0)
                    return simpleName;

                return new GenericNameBindingParserNode(simpleName.NameToken, TypeArguments);
            }
            else if (TypeOrFunction is MemberAccessBindingParserNode memberAccess)
            {
                if (TypeArguments.Count == 0)
                    return memberAccess;

                var genericName = new GenericNameBindingParserNode(memberAccess.MemberNameExpression.NameToken, TypeArguments);
                memberAccess.MemberNameExpression = genericName;
                return memberAccess;
            }

            throw new InvalidOperationException($"Can not convert {TypeOrFunction} to function!");
        }
    }
}
