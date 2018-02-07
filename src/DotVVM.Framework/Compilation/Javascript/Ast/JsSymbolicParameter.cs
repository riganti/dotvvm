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
        private CodeSymbolicParamerer symbol;

        public CodeSymbolicParamerer Symbol
        {
            get { return symbol; }
            set { ThrowIfFrozen(); symbol = value; }
        }

        public JsSymbolicParameter(CodeSymbolicParamerer symbol, CodeParameterAssignment? defaultAssignment = null)
        {
            this.symbol = symbol;
            this.defaultAssignment = defaultAssignment;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitSymbolicParameter(this);
    }
}
