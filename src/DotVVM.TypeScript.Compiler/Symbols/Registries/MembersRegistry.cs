using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Symbols.Registries
{
    public class MembersRegistry
    {
        public INamedTypeSymbol Type { get; }
        public bool WasResolved { get; set; }
        public IList<MemberInfo> Members { get;  }


        public MembersRegistry(INamedTypeSymbol type) : this(type, new List<MemberInfo>()) { }

        public MembersRegistry(INamedTypeSymbol type, IEnumerable<MemberInfo> members)
        {
            Type = type;
            Members = members.ToList();
        }
        
    }
}
