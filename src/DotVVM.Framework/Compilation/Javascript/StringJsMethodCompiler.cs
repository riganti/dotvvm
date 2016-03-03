using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class StringJsMethodCompiler: IJsMethodTranslator
    {
        public string FormatString { get; set; }

        public string TranslateCall(string context, string[] arguments, MethodInfo method)
        {
            return string.Format(FormatString, new[] { context }.Concat(arguments).ToArray());
        }

        public StringJsMethodCompiler(string formatString)
        {
            FormatString = formatString;
        }
    }
}
