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
        /// <summary> Invoke a command in client-side module registered for this page or control. The command cannot return a promise, use <see cref="InvokeAsync" /> for that. </summary>
        public void Invoke(string name, params object[] args) =>
            throw new Exception($"Cannot invoke JS command server-side: {name}({string.Join(", ", args)}).");
        /// <summary> Invoke a command in client-side module registered for this page or control. <typeparamref name="T"/> is the return type. The command cannot return a promise, use <see cref="InvokeAsync{T}" /> for that. </summary>
        public T Invoke<T>(string name, params object[] args) =>
            throw new Exception($"Cannot invoke JS command server-side: {name}({string.Join(", ", args)}) -> {typeof(T).Name}.");

        /// <summary> Invoke a command in client-side module registered for this page or control. The command can return a Promise. </summary>
        public void InvokeAsync(string name, params object[] args) =>
            throw new Exception($"Cannot invoke JS async command server-side: {name}({string.Join(", ", args)}).");
        /// <summary> Invoke a command in client-side module registered for this page or control. <typeparamref name="T"/> is the return type. The command can return a Promise. </summary>
        public T InvokeAsync<T>(string name, params object[] args) =>
            throw new Exception($"Cannot invoke JS async command server-side: {name}({string.Join(", ", args)}) -> {typeof(T).Name}.");

        internal static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection collection)
        {
            var compiler = new GenericMethodCompiler(
                (a, method) => {
                    var annotation = a[0].Annotation<JsExtensionParameter.ViewModuleAnnotation>().NotNull("invalid call of _js.Invoke");
                    var viewIdOrElementExpr = annotation.IsMarkupControl ? new JsSymbolicParameter(JavascriptTranslator.CurrentElementParameter) : (JsExpression)new JsLiteral(annotation.Id);

                    var isAsync = typeof(Task).IsAssignableFrom(method.ReturnType) || method.Name == "InvokeAsync";

                    var jsExpression = new JsInvocationExpression(a[0].Member("call"), viewIdOrElementExpr, a[1], a[2]);
                    if (isAsync)
                    {
                        jsExpression.AddAnnotation(new ResultIsPromiseAnnotation(e => e));
                    }
                    else
                    {
                        // client-side check that the invocation does not return a Promise
                        jsExpression.Arguments.Add(new JsLiteral(false));
                    }
                    return jsExpression;
                });
            collection.AddMethodTranslator(typeof(JsBindingApi), nameof(Invoke), compiler, null, true, true);
            collection.AddMethodTranslator(typeof(JsBindingApi), nameof(InvokeAsync), compiler, null, true, true);
        }
    }
}
