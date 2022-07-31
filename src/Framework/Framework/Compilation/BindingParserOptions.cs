using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Compilation
{
    public class BindingParserOptions
    {
        public Type BindingType { get; }
        public string ScopeParameter { get; }

        /// <summary>
        /// Additional namespace imports that will be added to imports defined in Dotvvm page just before the binding is resolved
        /// </summary>
        public ImmutableArray<NamespaceImport> ImportNamespaces { get; }

        /// <summary>
        /// Additional export parameters that will be added to export parameters defined in Dotvvm page just before the binding is resolved
        /// </summary>
        public ImmutableArray<BindingExtensionParameter> ExtensionParameters { get; }

        public virtual TypeRegistry AddImportedTypes(TypeRegistry reg, CompiledAssemblyCache compiledAssemblyCache)
        {
            return ImportNamespaces != null
                ? reg.AddImportedTypes(compiledAssemblyCache, ImportNamespaces)
                : reg;
        }

        public BindingParserOptions(Type bindingType, string scopeParameter = "_this", ImmutableArray<NamespaceImport>? importNamespaces = null, ImmutableArray<BindingExtensionParameter>? extParameters = null)
        {
            BindingType = bindingType;
            ScopeParameter = scopeParameter;
            ImportNamespaces = importNamespaces ?? ImmutableArray<NamespaceImport>.Empty;
            ExtensionParameters = extParameters ?? ImmutableArray<BindingExtensionParameter>.Empty;
        }

        public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this", IEnumerable<NamespaceImport>? importNs = null, ImmutableArray<BindingExtensionParameter>? extParameters = null)
            => new BindingParserOptions(typeof(TBinding), scopeParameter, 
                importNamespaces: importNs?.ToImmutableArray(),
                extParameters: extParameters);

        public static BindingParserOptions Create(Type bindingType, string scopeParameter = "_this", IEnumerable<NamespaceImport>? importNs = null, ImmutableArray<BindingExtensionParameter>? extParameters = null)
            => new BindingParserOptions(bindingType, scopeParameter,
                importNamespaces: importNs?.ToImmutableArray(), 
                extParameters: extParameters).AddParameters(new [] { new CurrentUserExtensionParameter() });

        public static readonly BindingParserOptions Value = Create(typeof(ValueBindingExpression<>));
        public static readonly BindingParserOptions ControlProperty = Create(typeof(ControlPropertyBindingExpression<>));
        public static readonly BindingParserOptions Resource = Create(typeof(ResourceBindingExpression<>));
        public static readonly BindingParserOptions Command = Create(typeof(CommandBindingExpression<>));
        public static readonly BindingParserOptions ControlCommand = Create(typeof(ControlCommandBindingExpression<>));
        public static readonly BindingParserOptions StaticCommand = Create(typeof(StaticCommandBindingExpression<>));

        public BindingParserOptions AddImports(params NamespaceImport[]? imports)
            => AddImports((IEnumerable<NamespaceImport>?)imports);
        public BindingParserOptions AddImports(IEnumerable<NamespaceImport>? imports)
        {
            if (imports == null)
                return this;
            var union = ImportNamespaces.Union(imports).ToImmutableArray();
            if (union.Length == ImportNamespaces.Length)
                return this;
            return new BindingParserOptions(BindingType, ScopeParameter, union, ExtensionParameters);
        }

        public BindingParserOptions AddParameters(IEnumerable<BindingExtensionParameter>? extParams)
            => extParams == null || !extParams.Any() ? this :
               new BindingParserOptions(BindingType, ScopeParameter, ImportNamespaces, ExtensionParameters.AddRange(extParams));

        public BindingParserOptions WithScopeParameter(string scopeParameter)
            => new BindingParserOptions(BindingType, scopeParameter, ImportNamespaces, ExtensionParameters);

        public override string ToString()
        {
            string?[] features = new [] {
                BindingType.Name,
                ImportNamespaces.Any() ? "imports=[" + string.Join(", ", this.ImportNamespaces) + "]" : null,
                ExtensionParameters.Any() ? "ext=[" + string.Join(", ", this.ExtensionParameters.Select(e => e.Identifier + ": " + e.ParameterType.Name)) + "]" : null,
                ScopeParameter != "_this" ? "scope=" + ScopeParameter : null,
            };
            return "{" + features.Where(a => a != null).StringJoin(", ") + "}";

        }
    }
}
