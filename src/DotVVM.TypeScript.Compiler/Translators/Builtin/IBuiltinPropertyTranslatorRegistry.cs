using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    interface IBuiltinPropertyTranslatorRegistry
    {
        IPropertyTranslator FindRegisteredTranslator(IPropertySymbol property);
        void RegisterTranslator(PropertyInfo propertyInfo, IPropertyTranslator translator);
    }
}
