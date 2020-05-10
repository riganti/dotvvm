#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public class BindingApi
    {
        public T RefreshOnChange<T>(T obj, object? refreshOn) => obj;

        public T RefreshOnEvent<T>(T obj, string eventName) => obj;

        public void PushEvent(string eventName) { }

        public static void RegisterJavascriptTranslations(JavascriptTranslatableMethodCollection methods)
        {
            methods.AddMethodTranslator(typeof(BindingApi), nameof(RefreshOnChange),
                new GenericMethodCompiler(a =>
                    new JsIdentifierExpression("dotvvm").Member("api").Member("refreshOn").Invoke(
                            a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                            a[2].WithAnnotation(ObservableTransformationAnnotation.EnsureWrapped))
                        .WithAnnotation(a[1].Annotation<ResultIsObservableAnnotation>())
                        .WithAnnotation(a[1].Annotation<ViewModelInfoAnnotation>())
                        .WithAnnotation(a[1].Annotation<MayBeNullAnnotation>())
                ));
            methods.AddMethodTranslator(typeof(BindingApi), nameof(RefreshOnEvent),
                new GenericMethodCompiler(a =>
                    new JsIdentifierExpression("dotvvm").Member("api").Member("refreshOn").Invoke(
                            a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                            new JsIdentifierExpression("dotvvm").Member("eventHub").Member("get").Invoke(a[2]))
                        .WithAnnotation(a[1].Annotation<ResultIsObservableAnnotation>())
                        .WithAnnotation(a[1].Annotation<ViewModelInfoAnnotation>())
                        .WithAnnotation(a[1].Annotation<MayBeNullAnnotation>())
                ));
            methods.AddMethodTranslator(typeof(BindingApi), nameof(PushEvent),
                new GenericMethodCompiler(a =>
                    new JsIdentifierExpression("dotvvm").Member("eventHub").Member("notify").Invoke(a[1])
                ));
        }
    }
}
