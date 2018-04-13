using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.TypeScript.Compiler.Symbols;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    class BuiltinPropertyTranslatorRegistry : IBuiltinPropertyTranslatorRegistry
    {
        private readonly Dictionary<PropertyInfo, IPropertyTranslator> _registeredTranslators = new Dictionary<PropertyInfo, IPropertyTranslator>();

        public IPropertyTranslator FindRegisteredTranslator(IPropertySymbol property)
        {
            var result = _registeredTranslators.Where(pair => property.IsEquivalentToProperty(pair.Key))
                .Select(e => (KeyValuePair<PropertyInfo, IPropertyTranslator>?)e)
                .FirstOrDefault();

            return result?.Value;
        }

        public void RegisterTranslator(PropertyInfo propertyInfo, IPropertyTranslator translator)
        {
            _registeredTranslators.Add(propertyInfo, translator);
        }
    }
}