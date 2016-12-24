using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsParenthesizedExpression : JsExpression
    {
        public JsExpression Expression
        {
            get { return GetChildByRole(JsTreeRoles.Expression); }
            set { SetChildByRole(JsTreeRoles.Expression, value); }
        }

        public JsParenthesizedExpression()
        {
        }

        public JsParenthesizedExpression(JsExpression expr)
        {
            Expression = expr;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitParenthesizedExpression(this);
    }
}
