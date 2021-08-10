using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsMemberAccessExpression : JsExpression
    {
        public JsExpression Target
        {
            get => GetChildByRole(JsTreeRoles.TargetExpression);
            set => SetChildByRole(JsTreeRoles.TargetExpression, value);
        }

        public string MemberName
        {
            get => GetChildByRole(JsTreeRoles.Identifier).Name;
            set => SetChildByRole(JsTreeRoles.Identifier, new JsIdentifier(value));
        }

        public JsIdentifier MemberNameToken
        {
            get => GetChildByRole(JsTreeRoles.Identifier);
            set => SetChildByRole(JsTreeRoles.Identifier, value);
        }

        public JsMemberAccessExpression() { }

        public JsMemberAccessExpression(JsExpression target, string memberName)
        {
            AddChild(target, JsTreeRoles.TargetExpression);
            this.MemberName = memberName;
        }

        public JsMemberAccessExpression(JsExpression target, JsIdentifier memberIdentifier)
        {
            AddChild(target, JsTreeRoles.TargetExpression);
            AddChild(memberIdentifier, JsTreeRoles.Identifier);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitMemberAccessExpression(this);
    }
}
