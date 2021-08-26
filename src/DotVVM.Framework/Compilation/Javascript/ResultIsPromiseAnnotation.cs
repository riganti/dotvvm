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
        /// <summary> If awaiting this expression is optional, in value bindings (synchronous context) it will be ignored. This is a hack for REST api bindings </summary>
        public bool IsOptionalAwait = false;
        /// <summary> If calling promiseGetter is optional when using await. This is a small optimization for not doing `await Promise.resolve` </summary>
        public bool IsPromiseGetterOptional = false;

        public ResultIsPromiseAnnotation(Func<JsExpression, JsExpression> promiseGetter, params object[] resultAnnotations)
        {
            this.ResultAnnotations = resultAnnotations.ToList();
            this.GetPromiseFromExpression = promiseGetter;
        }
    }
}
