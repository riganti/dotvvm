using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.Generic;

namespace DotVVM.Framework.Compilation
{
	public class BindingParserOptions
	{
		public Type BindingType { get; }
		public string ScopeParameter { get; }

		public NamespaceImport[] ImportNamespaces { get; set; }

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
					if (t.Length > import.Alias.Length + 1 && t.StartsWith(import.Alias, StringComparison.Ordinal) && t[import.Alias.Length] == '.')
						return TypeRegistry.CreateStatic(ReflectionUtils.FindType(import.Namespace + "." + t.Substring(import.Alias.Length + 1)));
					else return null;
				};
			else return t => TypeRegistry.CreateStatic(ReflectionUtils.FindType(import.Namespace + "." + t));
		}

		public BindingParserOptions(Type bindingType, string scopeParameter = "_this")
		{
			BindingType = bindingType;
			ScopeParameter = scopeParameter;
		}

		public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this", NamespaceImport[] importNs = null)
			=> new BindingParserOptions(typeof(TBinding), scopeParameter) { ImportNamespaces = importNs };
	}
}
