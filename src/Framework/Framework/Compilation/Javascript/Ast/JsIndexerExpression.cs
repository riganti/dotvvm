using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsIndexerExpression: JsExpression
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

        private bool isOptional;
        /// <summary> If true, `?.[` operator will be used. If false, `[` is used (this is default) </summary>
        public bool IsOptional
        {
            get { return isOptional; }
            set { ThrowIfFrozen(); isOptional = value; }
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
