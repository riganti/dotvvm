#nullable enable
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Binding
{
    public class BindingPageInfo
    {
        public bool IsPostbackRunning => false;
        public bool EvaluatingOnServer => true;
        public bool EvaluatingOnClient => false;

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection methods)
        {
            methods.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(EvaluatingOnServer),
                new GenericMethodCompiler(_ => new JsLiteral(false)));
            methods.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(EvaluatingOnClient),
                new GenericMethodCompiler(_ => new JsLiteral(true)));
            methods.AddPropertyGetterTranslator(typeof(BindingPageInfo), nameof(IsPostbackRunning),
                new GenericMethodCompiler(_ => new JsIdentifierExpression("dotvvm").Member("isPostbackRunning").Invoke()));
        }
    }
}
