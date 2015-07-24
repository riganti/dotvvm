using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public interface IJsMethodTranslator
    {
        string TranslateCall(string context, string[] arguments, MethodInfo method);
    }
}
