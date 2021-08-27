using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsReturnStatement : JsStatement
    {
        public JsExpression Expression
        {
            get { return GetChildByRole(JsTreeRoles.Expression)!; }
            set { SetChildByRole(JsTreeRoles.Expression, value); }
        }

        public JsReturnStatement()
        {
        }

        public JsReturnStatement(JsExpression expr)
        {
            Expression = expr;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitReturnStatement(this);
    }
}
