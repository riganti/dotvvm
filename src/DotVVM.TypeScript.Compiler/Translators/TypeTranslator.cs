using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.TypeScript.Compiler.Ast;
using DotVVM.TypeScript.Compiler.Symbols.Registries;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    public class TypeTranslator
    {
        private readonly TypeRegistry _typeRegistry;

        public TypeTranslator(TypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;
        }

        public IEnumerable<TsSyntaxTree> Translate()
        {
            List<TsSyntaxTree> trees = new List<TsSyntaxTree>();
            foreach (var type in _typeRegistry.Types.Where(t => t.WasResolved == false).ToList())
            {
                var classDeclaration = CreateClassDeclaration(type.Type);
                if(type.Type.BaseType != null && _typeRegistry.Types.Any(t => t.Type.Equals(type.Type.BaseType) == false))
                    _typeRegistry.RegisterType(type.Type.BaseType);
                type.WasResolved = true;
                trees.Add(new TsSyntaxTree(classDeclaration));
            }

            if (_typeRegistry.Types.Any(t => t.WasResolved))
            {
                trees.AddRange(Translate());
            }

            return trees;
        }

        private TsClassDeclarationSyntax CreateClassDeclaration(ITypeSymbol typeSymbol)
        {
            var identifier = CreateIdentifierSyntax(typeSymbol.Name);
            //var baseType = CreateIdentifierSyntax(typeSymbol.BaseType.Name);
            var interfaces = typeSymbol.Interfaces.Select(i => CreateIdentifierSyntax(i.Name));
            return new TsClassDeclarationSyntax(identifier, new List<TsMemberDeclarationSyntax>(), new List<TsIdentifierSyntax>() {}, null);
        }

        private TsIdentifierSyntax CreateIdentifierSyntax(string name, TsSyntaxNode parent = null)
        {
            return new TsIdentifierSyntax(name, parent);
        }

    }
}
