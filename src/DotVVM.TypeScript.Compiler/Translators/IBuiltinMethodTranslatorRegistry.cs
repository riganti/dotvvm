using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace DotVVM.TypeScript.Compiler.Translators
{
    interface IBuiltinMethodTranslatorRegistry
    {
        IMethodCallTranslator FindRegisteredMethod(IMethodSymbol methodSymbol);
        void RegisterMethod(MethodInfo methodInfo, IMethodCallTranslator equivalent);
    }
}
