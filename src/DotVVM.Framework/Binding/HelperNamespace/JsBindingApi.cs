using System;
using System.Threading.Tasks;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public class JsBindingApi
    {
        public void Invoke(string name, params object[] args) =>
            throw new Exception($"Can not invoke JS command server-side: {name}({string.Join(", ", args)}).");
        public T Invoke<T>(string name, params object[] args) =>
            throw new Exception($"Can not invoke JS command server-side: {name}({string.Join(", ", args)}) -> {typeof(T).Name}.");

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection collection)
        {
            collection.AddMethodTranslator(typeof(JsBindingApi), nameof(Invoke), new GenericMethodCompiler(
                (a, method) => {
                    var annotation = a[0].Annotation<JsExtensionParameter.ViewModuleAnnotation>();
                    var viewIdExpr = annotation.IsMarkupControl ? new JsSymbolicParameter(CommandBindingExpression.ControlUniqueIdParameter) : (JsExpression)new JsLiteral(annotation.Id);
                    
                    var jsExpression = a[0].Member("call").Invoke(viewIdExpr, a[1], a[2]);
                    if (typeof(Task).IsAssignableFrom(method.ReturnType))
                    {
                        jsExpression = jsExpression.WithAnnotation(new ResultIsPromiseAnnotation(e => e));
                    }
                    return jsExpression;
                }), null, true, true);
        }
    }
}
