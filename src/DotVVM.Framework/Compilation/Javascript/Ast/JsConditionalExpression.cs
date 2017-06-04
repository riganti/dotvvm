using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsConditionalExpression: JsExpression
    {
        public static JsTreeRole<JsExpression> ConditionRole => JsTreeRoles.Condition;
        public readonly static JsTreeRole<JsExpression> TrueRole = new JsTreeRole<JsExpression>("True");
        public readonly static JsTreeRole<JsExpression> FalseRole = new JsTreeRole<JsExpression>("False");
        public JsExpression Condition
        {
            get { return GetChildByRole(JsTreeRoles.Condition); }
            set { SetChildByRole(JsTreeRoles.Condition, value); }
        }

        public JsExpression TrueExpression
        {
            get { return GetChildByRole(TrueRole); }
            set { SetChildByRole(TrueRole, value); }
        }
        public JsExpression FalseExpression
        {
            get { return GetChildByRole(FalseRole); }
            set { SetChildByRole(FalseRole, value); }
        }
        public JsConditionalExpression()
        {
        }

        public JsConditionalExpression(JsExpression condition, JsExpression trueExpression, JsExpression falseExpression)
        {
            AddChild(condition, JsTreeRoles.Condition);
            AddChild(trueExpression, TrueRole);
            AddChild(falseExpression, FalseRole);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitConditionalExpression(this);
    }
}
