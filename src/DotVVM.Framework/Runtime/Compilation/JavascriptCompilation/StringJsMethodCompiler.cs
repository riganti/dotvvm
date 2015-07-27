using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
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
