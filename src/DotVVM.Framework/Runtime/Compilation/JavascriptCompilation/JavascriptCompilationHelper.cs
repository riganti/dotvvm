using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public static class JavascriptCompilationHelper
    {
        public static string CompileConstant(object obj)
            => JsonConvert.SerializeObject(obj);
    }
}
