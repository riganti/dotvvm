using System;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public class JsBindingApi
    {
        public void Invoke(string name, params object[] args) =>
            throw new Exception("Can not invoke JS command server-side.");
        public T Invoke<T>(string name, params object[] args) =>
            throw new Exception("Can not invoke JS command server-side.");

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection collection)
        {
            collection.AddMethodTranslator(typeof(JsBindingApi), nameof(Invoke), new GenericMethodCompiler(
                a => {
                    var viewId = a[0].Annotation<JsExtensionParameter.ViewModuleAnnotation>().Id;

                    return a[0].Member("call")
                    .Invoke(new JsLiteral(viewId), a[1], a[2]);
                }), null, true, true);
        }
    }
}
