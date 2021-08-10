using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsExpressionStatement : JsStatement
    {
        public JsExpression Expression
        {
            get { return GetChildByRole(JsTreeRoles.Expression); }
            set { SetChildByRole(JsTreeRoles.Expression, value); }
        }

        public JsExpressionStatement()
        {
        }

        public JsExpressionStatement(JsExpression expr)
        {
            Expression = expr;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitExpressionStatement(this);
    }
}
