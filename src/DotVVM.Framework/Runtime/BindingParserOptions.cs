using DotVVM.Framework.Runtime.Compilation.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime
{
    public class BindingParserOptions
    {
        public Type BindingType { get; }
        public string ScopeParameter { get; }
        public virtual TypeRegistry AddTypes(TypeRegistry reg) => reg;

        public BindingParserOptions(Type bindingType, string scopeParameter = "_this")
        {
            BindingType = bindingType;
            ScopeParameter = scopeParameter;
        }

        public static BindingParserOptions Create<TBinding>(string scopeParameter = "_this")
            => new BindingParserOptions(typeof(TBinding), scopeParameter);
    }
}
