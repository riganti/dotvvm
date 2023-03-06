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
        /// <summary> Returns true if any command or staticCommand is currently running. Always returns false on the server. </summary>
        public bool IsPostbackRunning => false;
        /// <summary> Returns true on server and false in JavaScript. </summary>
        public bool EvaluatingOnServer => true;
        /// <summary> Returns false on server and true in JavaScript. </summary>
        public bool EvaluatingOnClient => false;

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection methods)
        {
            methods.AddPropertyTranslator(() => new BindingPageInfo().EvaluatingOnServer,
                new GenericMethodCompiler(_ => new JsLiteral(false)));
            methods.AddPropertyTranslator(() => new BindingPageInfo().EvaluatingOnClient,
                new GenericMethodCompiler(_ => new JsLiteral(true)));
            methods.AddPropertyTranslator(() => new BindingPageInfo().IsPostbackRunning,
                new GenericMethodCompiler(_ => new JsIdentifierExpression("dotvvm").Member("isPostbackRunning").Invoke()));
        }
    }
}
