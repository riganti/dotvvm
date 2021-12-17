using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsSymbolicParameter : JsExpression
    {
        private CodeParameterAssignment? defaultAssignment;
        public CodeParameterAssignment? DefaultAssignment
        {
            get { return defaultAssignment;}
            set { ThrowIfFrozen(); defaultAssignment = value;}
        }

        public CodeParameterAssignment? GetDefaultAssignment()
        {
            if (DefaultAssignment != null)
                return DefaultAssignment;
            if (Symbol.HasDefault)
                return Symbol.DefaultAssignment;
            return null;
        }

        private CodeSymbolicParameter symbol;

        public CodeSymbolicParameter Symbol
        {
            get { return symbol; }
            set { ThrowIfFrozen(); symbol = value; }
        }

        public IEnumerable<CodeSymbolicParameter> EnumerateAllSymbols()
        {
            yield return Symbol;
            if (GetDefaultAssignment() is CodeParameterAssignment d)
                foreach (var inDefault in d.Code!.EnumerateAllParameters())
                    yield return inDefault;
        }

        public JsSymbolicParameter(CodeSymbolicParameter symbol, CodeParameterAssignment? defaultAssignment = null)
        {
            this.symbol = symbol;
            this.defaultAssignment = defaultAssignment;
        }

        public static JsSymbolicParameter CreateCodePlaceholder(ParametrizedCode code) =>
            new JsSymbolicParameter(
                new CodeSymbolicParameter("AdHoc placeholder"),
                new CodeParameterAssignment(code)
            );

        public static JsSymbolicParameter CreateCodePlaceholder(string code, OperatorPrecedence operatorPrecedence) =>
            new JsSymbolicParameter(
                new CodeSymbolicParameter("AdHoc placeholder"),
                new CodeParameterAssignment(code, operatorPrecedence)
            );


        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitSymbolicParameter(this);
    }
}
