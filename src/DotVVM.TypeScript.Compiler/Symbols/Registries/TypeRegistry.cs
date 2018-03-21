using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Registries
{
    public class TypeRegistry
    {
        public IList<MembersRegistry> Types { get; }

        public TypeRegistry()
        {
            Types = new List<MembersRegistry>();
        }

        public void RegisterType(INamedTypeSymbol type)
        {
            Types.Add(new MembersRegistry(type));
        }

        public void RegisterType(INamedTypeSymbol type, IEnumerable<ISymbol> members)
        {
            var memberInfos = members.Select(m => new MemberInfo(m));
            Types.Add(new MembersRegistry(type, memberInfos));
        }

    }
}