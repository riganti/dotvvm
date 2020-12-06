using System;
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
                a =>
                a[0]
                .Member((a[1] as JsLiteral).Value?.ToString())
                .Invoke(a[2])
            ), null, true, true);
        }
    }
}
