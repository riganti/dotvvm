using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsIndexerExpression: JsExpression
    {
        public JsExpression Target
        {
            get => GetChildByRole(JsTreeRoles.TargetExpression)!;
            set => SetChildByRole(JsTreeRoles.TargetExpression, value);
        }

        public JsExpression Argument
        {
            get => GetChildByRole(JsTreeRoles.Argument)!;
            set => SetChildByRole(JsTreeRoles.Argument, value);
        }

        public JsIndexerExpression() { }

        public JsIndexerExpression(JsExpression target, JsExpression argument)
        {
            AddChild(target, JsTreeRoles.TargetExpression);
            AddChild(argument, JsTreeRoles.Argument);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIndexerExpression(this);
    }
}
