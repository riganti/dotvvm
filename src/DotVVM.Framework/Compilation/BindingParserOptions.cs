using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Compilation
{
    public class BindingParserOptions
    {
        public Type BindingType { get; }
        public string ScopeParameter { get; }

        /// <summary>
        /// Additional namespace imports that will be added to imports defined in Dotvvm page just before the binding is resolved
        /// </summary>
        public ImmutableList<NamespaceImport> ImportNamespaces { get; }

        /// <summary>
        /// Additional export parameters that will be added to export parameters defined in Dotvvm page just before the binding is resolved
        /// </summary>
        public ImmutableList<BindingExtensionParameter> ExtensionParameters { get; }

        public virtual TypeRegistry AddImportedTypes(TypeRegistry reg, CompiledAssemblyCache compiledAssemblyCache)
        {
            if (ImportNamespaces != null)
            {
                return reg.AddSymbols(ImportNamespaces.Select(ns => CreateTypeLoader(ns, compiledAssemblyCache)));
            }
            else return reg;
        }

        private static Func<string, Expression> CreateTypeLoader(NamespaceImport import, CompiledAssemblyCache compiledAssemblyCache)
        {
            if (import.HasAlias)
                return t => {
                    if (t.Length >= import.Alias.Length && t.StartsWith(import.Alias, StringComparison.Ordinal))
                    {
                        string name;
                        if (t == import.Alias) name = import.Namespace;
                        else if (t.Length > import.Alias.Length + 1 && t[import.Alias.Length] == '.') name = import.Namespace + "." + t.Substring(import.Alias.Length + 1);
                        else return null;
                        return TypeRegistry.CreateStatic(compiledAssemblyCache.FindType(name));
                    }
                    else return null;
                };
            else return t => TypeRegistry.CreateStatic(compiledAssemblyCache.FindType(import.Namespace + "." + t));
        }

        public BindingParserOptions(Type bindingType, string scopeParameter = "_this", ImmutableList<NamespaceImport> importNamespaces = null, ImmutableList<BindingExtensionParameter> extParameters = null)
        {
            BindingType = bindingType;
            ScopeParameter = scopeParameter;
            ImportNamespaces = importNamespaces ?? ImmutableList<NamespaceImport>.Empty;
            ExtensionParameters = extParameters ?? ImmutableList<BindingExtensionParameter>.Empty;
        }

        public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this", IEnumerable<NamespaceImport> importNs = null, ImmutableList<BindingExtensionParameter> extParameters = null)
            => new BindingParserOptions(typeof(TBinding), scopeParameter, 
                importNamespaces: importNs?.ToImmutableList(),
                extParameters: extParameters);

        public static BindingParserOptions Create(Type bindingType, string scopeParameter = "_this", IEnumerable<NamespaceImport> importNs = null, ImmutableList<BindingExtensionParameter> extParameters = null)
            => new BindingParserOptions(bindingType, scopeParameter,
                importNamespaces: importNs?.ToImmutableList(), 
                extParameters: extParameters);

        public BindingParserOptions AddImports(params NamespaceImport[] imports)
            => AddImports((IEnumerable<NamespaceImport>)imports);
        public BindingParserOptions AddImports(IEnumerable<NamespaceImport> imports)
            => new BindingParserOptions(BindingType, ScopeParameter, ImportNamespaces.AddRange(imports), ExtensionParameters);

        public BindingParserOptions AddParameters(IEnumerable<BindingExtensionParameter> extParams)
            => new BindingParserOptions(BindingType, ScopeParameter, ImportNamespaces, ExtensionParameters.AddRange(extParams));

        public BindingParserOptions WithScopeParameter(string scopeParameter)
            => new BindingParserOptions(BindingType, scopeParameter, ImportNamespaces, ExtensionParameters);
    }
}
