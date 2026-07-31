using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;
using System.Collections.Generic;
using System.Collections.Immutable;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding.Expressions;
using FastExpressionCompiler;
using System.Net;
using System.Collections;

namespace DotVVM.Framework.Compilation
{
    public class BindingParserOptions : IDebugHtmlFormattableObject, IEquatable<BindingParserOptions>
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
                extParameters: extParameters).AddParameters(new[] { new CurrentUserExtensionParameter() });

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

        public override int GetHashCode() =>
            (
                this.ScopeParameter.GetHashCode(),
                this.BindingType.GetHashCode(),
                StructuralComparisons.StructuralEqualityComparer.GetHashCode(this.ExtensionParameters),
                StructuralComparisons.StructuralEqualityComparer.GetHashCode(this.ImportNamespaces)
            ).GetHashCode();

        public override bool Equals(object? other) =>
            other is BindingParserOptions otherT && this.Equals(otherT);

        public bool Equals(BindingParserOptions? other) =>
            other is {} &&
            other.BindingType == this.BindingType &&
            other.ScopeParameter == this.ScopeParameter &&
            other.ImportNamespaces.SequenceEqual(this.ImportNamespaces) &&
            other.ExtensionParameters.SequenceEqual(this.ExtensionParameters);

        public override string ToString()
        {
            string?[] features = new[] {
                BindingType.ToCode(stripNamespace: true),
                ImportNamespaces.Any() ? "imports=[" + string.Join(", ", this.ImportNamespaces) + "]" : null,
                ExtensionParameters.Any() ? "ext=[" + string.Join(", ", this.ExtensionParameters.Select(e => e.Identifier + ": " + e.ParameterType.Name)) + "]" : null,
                ScopeParameter != "_this" ? "scope=" + ScopeParameter : null,
            };
            return "{" + features.Where(a => a != null).StringJoin(", ") + "}";

        }

        public string DebugHtmlString(IFormatProvider? formatProvider, bool isBlock)
        {
            List<string> result = [
                isBlock ? "<ul>" : "<span>{",
                isBlock ? "<li>type = <b>" : "<b>",
                BindingType.DebugHtmlString(false, true),
                isBlock ? "</b></li>" : "</b>: "
            ];

            if (ImportNamespaces.Any())
                result.AddRange([
                    isBlock ? "<li>" : "",
                    "<b>imports</b> =", isBlock ? " " : "[",
                    string.Join(", ", this.ImportNamespaces.Select(i =>
                        i.HasAlias ? $"<span class=syntax-class>{WebUtility.HtmlEncode(i.Alias)}</span>={WebUtility.HtmlEncode(i.Namespace)}"
                                   : $"{WebUtility.HtmlEncode(i.Namespace)}")),
                    isBlock ? "</li>" : "]"
                ]);

            if (ExtensionParameters.Any())
                result.AddRange([
                    isBlock ? "<li>" : "",
                    "<b>ext</b> =", isBlock ? " " : "[",
                    string.Join(", ", this.ExtensionParameters.Select(e =>
                        $"{WebUtility.HtmlEncode(e.Identifier)}: {e.ParameterType.DebugHtmlString(false, true)}")),
                    isBlock ? "</li>" : "]"
                ]);

            if (ScopeParameter != "_this")
                result.AddRange([
                    isBlock ? "<li>" : "",
                    "<b>scope</b> =", WebUtility.HtmlEncode(ScopeParameter),
                    isBlock ? "</li>" : ""
                ]);

            result.Add(isBlock ? "</ul>" : "}");

            return string.Concat(result);
        }
    }
}
