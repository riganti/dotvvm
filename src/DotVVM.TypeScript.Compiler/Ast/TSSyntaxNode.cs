using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Utils;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public class TsSyntaxTree
    {
        public TsSyntaxNode RootNode { get; }

        public TsSyntaxTree(TsSyntaxNode rootNode)
        {
            RootNode = rootNode;
        }
    }

    public abstract class TsSyntaxNode
    {
        protected TsSyntaxNode(TsSyntaxNode parent)
        {
            this.Parent = parent;
        }

        public TsSyntaxNode Parent { get; }

        public abstract string ToDisplayString();
        public abstract IEnumerable<TsSyntaxNode> DescendantNodes();

        public override string ToString()
        {
            return ToDisplayString();
        }
    }

    public class TsTypeSyntax : TsSyntaxNode
    {
        public ITypeSymbol EquivalentSymbol { get; }

        public TsTypeSyntax(ITypeSymbol equivalentSymbol, TsSyntaxNode parent) : base(parent)
        {
            EquivalentSymbol = equivalentSymbol;
        }

        public override string ToDisplayString()
        {
            return EquivalentSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }


    public class TsPropertyDeclarationSyntax : TsMemberDeclarationSyntax
    {
        public TsTypeSyntax Type { get; }

        public TsPropertyDeclarationSyntax(TsModifier modifier, TsIdentifierSyntax identifier, TsTypeSyntax type, TsSyntaxNode parent) : base(modifier, identifier, parent)
        {
            Type = type;
        }

        public override string ToDisplayString()
        {
            return $"{Modifier} {Identifier.ToDisplayString()}: {Type.ToDisplayString()}";
        }
        
        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override TsModifier Modifier { get; protected set; }
        public override TsIdentifierSyntax Identifier { get; set; }
    }


    public abstract class TsMemberDeclarationSyntax : TsSyntaxNode
    {
        protected TsMemberDeclarationSyntax(TsModifier modifier, TsIdentifierSyntax identifier, TsSyntaxNode parent) : base(parent)
        {
            Modifier = modifier;
            Identifier = identifier;
        }

        public abstract TsModifier Modifier { get; protected set; }
        public abstract TsIdentifierSyntax Identifier { get; set; }
    }

    public enum TsModifier
    {
        Public,
        Protected,
        Private
    }

    public class TsClassDeclarationSyntax : TsSyntaxNode
    {
        public TsIdentifierSyntax Identifier { get; }
        public IList<TsMemberDeclarationSyntax> Members { get; }
        public IList<TsIdentifierSyntax> BaseClasses { get; }

        public TsClassDeclarationSyntax(TsIdentifierSyntax identifier, IList<TsMemberDeclarationSyntax> members, IList<TsIdentifierSyntax> baseClasses, TsSyntaxNode parent) : base(parent)
        {
            Identifier = identifier;
            Members = members;
            BaseClasses = baseClasses;
        }

        public override string ToDisplayString()
        {
            var output = $"class {Identifier} ";
            if (BaseClasses.Any())
            {
                output += $"extends {BaseClasses.Select(i => i.ToDisplayString()).StringJoin(",")}";
            }
            output += " {\n";
            output += Members.Select(m => m.ToDisplayString()).StringJoin("\n");
            output += "\n}";
            return output;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Members;
        }

        public void AddMember(TsMemberDeclarationSyntax member)
        {
            Members.Add(member);
        }
    }

    public class TsIdentifierSyntax : TsSyntaxNode
    {
        public string Value { get; }

        public TsIdentifierSyntax(string value, TsSyntaxNode parent) : base(parent)
        {
            Value = value;
        }

        public override string ToDisplayString()
        {
            return Value;
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }

}
