#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.Parser.Binding.Parser
{
    public enum TypeModifier
    {
        None,
        Array,
        Nullable,        
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class TypeDeclarationBindingParserNode : BindingParserNode
    {
        public readonly TypeModifier Modifier;
        public readonly BindingParserNode? Type;
        public TypeDeclarationBindingParserNode? Next { get; set; }

        public TypeDeclarationBindingParserNode(BindingParserNode? type, TypeModifier modifier = TypeModifier.None, TypeDeclarationBindingParserNode? next = null)
        {
            this.Type = type;
            this.Modifier = modifier;
            this.Next = next;
        }

        public override IEnumerable<BindingParserNode> EnumerateChildNodes()
        {
            if (Type != null)
                yield return Type;

            var current = Next;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public override string ToDisplayString()
            => Type.ToDisplayString();
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class ArrayDeclarationBindingParserNode : TypeDeclarationBindingParserNode
    {
        public ArrayDeclarationBindingParserNode(BindingParserNode type)
            : base(null, TypeModifier.Array)
        {
            if (!(type is TypeDeclarationBindingParserNode innerTypeDecl))
                innerTypeDecl = new TypeDeclarationBindingParserNode(type);

            Next = innerTypeDecl;
        }

        public override string ToDisplayString() => $"{Next!.ToDisplayString()}[]";
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class NullableDeclarationBindingParserNode : TypeDeclarationBindingParserNode
    {
        public NullableDeclarationBindingParserNode(BindingParserNode type)
            : base(null, TypeModifier.Nullable)
        {
            if (!(type is TypeDeclarationBindingParserNode innerTypeDecl))
                innerTypeDecl = new TypeDeclarationBindingParserNode(type);

            Next = innerTypeDecl;
        }

        public override string ToDisplayString() => $"{Next!.ToDisplayString()}?";
    }
}
