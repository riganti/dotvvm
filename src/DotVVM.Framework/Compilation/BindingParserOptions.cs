using System;
using System.Linq;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation
{
    public class BindingParserOptions
    {
        public Type BindingType { get; }
        public string ScopeParameter { get; }

        public string[] ImportNamespaces { get; set; } = new string[0];

        public virtual TypeRegistry AddTypes(TypeRegistry reg) => reg.AddSymbols(ImportNamespaces.Select(n =>
            (Func<string, Expression>)(t => TypeRegistry.CreateStatic(ReflectionUtils.FindType(n + "." + t)))));

        public BindingParserOptions(Type bindingType, string scopeParameter = "_this")
        {
            BindingType = bindingType;
            ScopeParameter = scopeParameter;
        }

        public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this", string[] importNs = null)
            => new BindingParserOptions(typeof(TBinding), scopeParameter) { ImportNamespaces = importNs ?? new string[0] };
    }
}
