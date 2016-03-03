using System.Reflection;

namespace DotVVM.Framework.Compilation.Javascript
{
    public interface IJsMethodTranslator
    {
        string TranslateCall(string context, string[] arguments, MethodInfo method);
    }
}
