using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Binding
{
    public class BindingPageInfo
    {
        public bool IsPostbackRunning => false;
        public bool EvaluatingOnServer => true;
        public bool EvaluatingOnClient => false;

        internal static void RegisterJavascriptTranslations()
        {
            JavascriptTranslator.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(EvaluatingOnServer), new StringJsMethodCompiler("false"));
            JavascriptTranslator.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(EvaluatingOnClient), new StringJsMethodCompiler("true"));
            JavascriptTranslator.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(IsPostbackRunning), new StringJsMethodCompiler("dotvvm.isPostbackRunning()"));
        }
    }
}
