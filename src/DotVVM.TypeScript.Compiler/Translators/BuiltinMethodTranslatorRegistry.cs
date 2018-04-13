using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.TypeScript.Compiler.Symbols;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    class BuiltinMethodTranslatorRegistry : IBuiltinMethodTranslatorRegistry
    {
        private readonly Dictionary<MethodInfo, IMethodCallTranslator> _registeredMethods = new Dictionary<MethodInfo, IMethodCallTranslator>();

        public IMethodCallTranslator FindRegisteredMethod(IMethodSymbol methodSymbol)
        {
            var result = _registeredMethods.Where(pair => methodSymbol.IsEquivalentToMethod(pair.Key))
                .Select(e => (KeyValuePair<MethodInfo, IMethodCallTranslator>?)e)
                .FirstOrDefault();

            return result?.Value;
        }
        
        public void RegisterMethod(MethodInfo methodInfo, IMethodCallTranslator equivalent)
        {
            _registeredMethods.Add(methodInfo, equivalent);
        }
    }
}
