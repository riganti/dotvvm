using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsSymbolicParameter : JsExpression
    {
        private CodeParameterAssignment? defaultAssignment;
        public CodeParameterAssignment? DefaultAssignment
        {
            get { return defaultAssignment;}
            set { ThrowIfFrozen(); defaultAssignment = value;}
        }
        private CodeSymbolicParameter symbol;

        public CodeSymbolicParameter Symbol
        {
            get { return symbol; }
            set { ThrowIfFrozen(); symbol = value; }
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
