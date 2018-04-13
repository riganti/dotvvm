using System.Reflection;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators.Builtin
{
    interface IBuiltinMethodTranslatorRegistry
    {
        IMethodCallTranslator FindRegisteredTranslator(IMethodSymbol methodSymbol);
        void RegisterTranslator(MethodInfo methodInfo, IMethodCallTranslator equivalent);
    }
}
