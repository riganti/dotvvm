using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation
{
    public class CodeEmitterBase
    {
        protected List<MemberDeclarationSyntax> otherDeclarations = new List<MemberDeclarationSyntax>();

        private ConcurrentDictionary<Assembly, string> usedAssemblies = new ConcurrentDictionary<Assembly, string>();

        private static int assemblyIdCtr = 0;

        public IEnumerable<UsedAssembly> UsedAssemblies
        {
            get { return usedAssemblies.Select(a => new UsedAssembly() { Assembly = a.Key, Identifier = a.Value }); }
        }

        public string UseType(Type type)
        {
            if (type == null) return null;
            UseType(type.GetTypeInfo().BaseType);
            return usedAssemblies.GetOrAdd(type.GetTypeInfo().Assembly, _ => "Asm_" + Interlocked.Increment(ref assemblyIdCtr));
        }


        protected TypeSyntax ParseTypeName(Type type)
        {
            var asmName = UseType(type);
            if (type == typeof(void))
            {
                return SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
            }
            else if (!type.GetTypeInfo().IsGenericType)
            {
                return SyntaxFactory.ParseTypeName($"{asmName}::{type.FullName.Replace('+', '.')}");
            }
            else
            {
                var fullName = type.GetGenericTypeDefinition().FullName;
                if (fullName.Contains("`"))
                {
                    fullName = fullName.Substring(0, fullName.IndexOf("`", StringComparison.Ordinal));
                }

                var parts = fullName.Split('.');
                NameSyntax identifier = SyntaxFactory.AliasQualifiedName(
                    SyntaxFactory.IdentifierName(asmName),
                    SyntaxFactory.IdentifierName(parts[0]));
                for (var i = 1; i < parts.Length - 1; i++)
                {
                    identifier = SyntaxFactory.QualifiedName(identifier, SyntaxFactory.IdentifierName(parts[i]));
                }

                var typeArguments = type.GetGenericArguments().Select(ParseTypeName);
                return SyntaxFactory.QualifiedName(identifier,
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier(parts[parts.Length - 1]),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(typeArguments.ToArray())
                        )
                    )
                );
            }
        }

    }
}
