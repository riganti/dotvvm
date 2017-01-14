using System;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DotVVM.Framework.Compilation.Javascript
{
    public static class JavascriptCompilationHelper
    {
        public static string CompileConstant(object obj) => JsonConvert.SerializeObject(obj, new StringEnumConverter());

        private static readonly object indexerTargetParameter = new object();
        private static readonly object indexerExpressionParameter = new object();
        private static readonly ParametrizedCode indexerCode =
            new JsIdentifierExpression("ko").Member("unwrap").Invoke(new JsSymbolicParameter(indexerTargetParameter)).Indexer(new JsSymbolicParameter(indexerExpressionParameter))
            .FormatParametrizedScript();
        public static ParametrizedCode AddIndexerToViewModel(ParametrizedCode script, object index, bool unwrap = false) =>
            AddIndexerToViewModel(script, new JsLiteral(index), unwrap);
        public static ParametrizedCode AddIndexerToViewModel(ParametrizedCode script, JsExpression indexer, bool unwrap = false)
        {
            // T+ use JsTree for this
            return indexerCode.AssignParameters(o =>
                o == indexerTargetParameter ? new CodeParameterAssignment(script) :
                o == indexerExpressionParameter ? CodeParameterAssignment.FromExpression(indexer) :
                default(CodeParameterAssignment));
            //if (!script.EndsWith("()", StringComparison.Ordinal))
            //{
            //    if (unwrap)
            //    {
            //        script = "ko.unwrap(" + script + ")";
            //    }
            //    else
            //    {
            //        script += "()";
            //    }
            //}
            //else
            //{
            //    script = "(" + script + ")";
            //}

            //return script + "[" + index + "]";
        }
    }
}
