using System.Collections.Generic;
using System.Linq;
using DotVVM.TypeScript.Compiler.Ast;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Registries
{
    public class TypeRegistry
    {
        public IList<ITypeSymbol> UnresolvedTypes { get; }
        public IDictionary<ITypeSymbol, ISyntaxNode> ResolvedTypes { get; }        

        public TypeRegistry()
        {
            UnresolvedTypes = new List<ITypeSymbol>();
            ResolvedTypes = new Dictionary<ITypeSymbol, ISyntaxNode>();
        }

        public void RegisterType(ITypeSymbol type)
        {
            if (UnresolvedTypes.Contains(type) || ResolvedTypes.ContainsKey(type))
                return;
            foreach (var generics in type.UnwrapGeneric().OfType<INamedTypeSymbol>())
            {
                RegisterType(generics);
            }
            if (type.IsBuiltinType())
                return;
            UnresolvedTypes.Add(type);
        }

        public void MarkResolved(ITypeSymbol type, ISyntaxNode node)
        {
            UnresolvedTypes.Remove(type);
            ResolvedTypes.Add(type, node);
        }

    }
}
