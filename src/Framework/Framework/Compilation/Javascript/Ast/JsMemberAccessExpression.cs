﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsMemberAccessExpression : JsExpression
    {
        public JsExpression Target
        {
            get => GetChildByRole(JsTreeRoles.TargetExpression)!;
            set => SetChildByRole(JsTreeRoles.TargetExpression, value);
        }

        public string MemberName
        {
            get => MemberNameToken.Name;
            set => MemberNameToken = new JsIdentifier(value);
        }

        private bool isOptional;
        /// <summary> If true, `?.` operator will be used. If false, `.` is used (this is default) </summary>
        public bool IsOptional
        {
            get { return isOptional; }
            set { ThrowIfFrozen(); isOptional = value; }
        }

        public JsIdentifier MemberNameToken
        {
            get => GetChildByRole(JsTreeRoles.Identifier)!;
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
