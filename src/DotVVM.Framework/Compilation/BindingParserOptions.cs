using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation
{
	public class BindingParserOptions
	{
		public Type BindingType { get; }
		public string ScopeParameter { get; }

		public ImmutableList<NamespaceImport> ImportNamespaces { get; }

		public virtual TypeRegistry AddTypes(TypeRegistry reg)
		{
			if (ImportNamespaces != null)
			{
				return reg.AddSymbols(ImportNamespaces.Select(CreateTypeLoader));
			}
			else return reg;
		}

		private static Func<string, Expression> CreateTypeLoader(NamespaceImport import)
		{
			if (import.HasAlias)
				return t =>
				{
					if (t.Length >= import.Alias.Length && t.StartsWith(import.Alias, StringComparison.Ordinal))
					{
						string name;
						if (t == import.Alias) name = import.Namespace;
						else if (t.Length > import.Alias.Length + 1 && t[import.Alias.Length] == '.') name = import.Namespace + "." + t.Substring(import.Alias.Length + 1);
						else return null;
						return TypeRegistry.CreateStatic(ReflectionUtils.FindType(name));
					}
					else return null;
				};
			else return t => TypeRegistry.CreateStatic(ReflectionUtils.FindType(import.Namespace + "." + t));
		}

		public BindingParserOptions(Type bindingType, string scopeParameter = "_this", ImmutableList<NamespaceImport> importNamespaces = null)
		{
			BindingType = bindingType;
			ScopeParameter = scopeParameter;
            ImportNamespaces = importNamespaces;
		}

		public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this", IEnumerable<NamespaceImport> importNs = null)
			=> new BindingParserOptions(typeof(TBinding), scopeParameter, importNamespaces: importNs is ImmutableList<NamespaceImport> || importNs == null ? (ImmutableList<NamespaceImport>)importNs : importNs.ToImmutableList());

        public BindingParserOptions AddImports(params NamespaceImport[] imports)
            => AddImports((IEnumerable<NamespaceImport>)imports);
        public BindingParserOptions AddImports(IEnumerable<NamespaceImport> imports)
            => new BindingParserOptions(BindingType, ScopeParameter, ImportNamespaces == null ? imports.ToImmutableList() : ImportNamespaces.AddRange(imports));

        public BindingParserOptions WithScopeParameter(string scopeParameter)
            => new BindingParserOptions(BindingType, scopeParameter, ImportNamespaces);
	}
}
