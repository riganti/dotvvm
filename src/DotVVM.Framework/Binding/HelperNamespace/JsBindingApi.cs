using System;
using DotVVM.Framework.Compilation.Javascript;

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
        }
    }
}
