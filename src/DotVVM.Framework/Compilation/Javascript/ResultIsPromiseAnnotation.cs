using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public sealed class ResultIsPromiseAnnotation
    {
        public Func<JsExpression, JsExpression> GetPromiseFromExpression;
        public List<object> ResultAnnotations;

        public ResultIsPromiseAnnotation(params object[] resultAnnotations): this(e => e, resultAnnotations) { }
        public ResultIsPromiseAnnotation(Func<JsExpression, JsExpression> promiseGetter, params object[] resultAnnotations)
        {
            this.ResultAnnotations = resultAnnotations.ToList();
            this.GetPromiseFromExpression = promiseGetter;
        }
    }
}
