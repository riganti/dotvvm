using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsInvocationExpression : JsExpression
    {
        public JsExpression Target
        {
            get { return GetChildByRole(JsTreeRoles.TargetExpression)!; }
            set { SetChildByRole(JsTreeRoles.TargetExpression, value); }
        }
        public JsNodeCollection<JsExpression> Arguments
        {
            get { return GetChildrenByRole<JsExpression>(JsTreeRoles.Argument); }
        }

        public JsInvocationExpression()
        {
        }

        public JsInvocationExpression(JsExpression target, IEnumerable<JsExpression?> arguments)
        {
            AddChild(target, JsTreeRoles.TargetExpression);
            if (arguments != null) {
                foreach (var arg in arguments) {
                    AddChild(arg, JsTreeRoles.Argument);
                }
            }
        }

        public JsInvocationExpression(JsExpression target, params JsExpression?[] arguments) : this(target, (IEnumerable<JsExpression?>)arguments)
        {
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitInvocationExpression(this);
    }
}
