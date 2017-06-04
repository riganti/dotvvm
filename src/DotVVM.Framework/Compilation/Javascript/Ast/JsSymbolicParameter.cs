using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsSymbolicParameter : JsExpression
    {
        private object symbol;

        public object Symbol
        {
            get { return symbol; }
            set { ThrowIfFrozen(); symbol = value; }
        }

        public JsSymbolicParameter(object symbol)
        {
            this.symbol = symbol;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitSymbolicParameter(this);
    }
}
