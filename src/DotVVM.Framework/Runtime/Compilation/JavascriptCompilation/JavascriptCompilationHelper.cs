using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public static class JavascriptCompilationHelper
    {
        public static string CompileConstant(object obj) => JsonConvert.SerializeObject(obj, new StringEnumConverter());

        public static string AddIndexerToViewModel(string script, object index, bool unwrap = false)
        {
            if (!script.EndsWith("()", StringComparison.Ordinal))
            {
                if (unwrap)
                {
                    script = "ko.unwrap(" + script + ")";
                }
                else
                {
                    script += "()";
                }
            }
            else
            {
                script = "(" + script + ")";
            }

            return script + "[" + index + "]";
        }
    }
}
