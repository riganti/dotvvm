using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    interface IBuiltinMethodTranslatorRegistry
    {
        IMethodCallTranslator FindRegisteredTranslator(IMethodSymbol methodSymbol);
        void RegisterTranslator(MethodInfo methodInfo, IMethodCallTranslator equivalent);
    }
}
