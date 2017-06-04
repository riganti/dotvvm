using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsIdentifierExpression: JsExpression
    {
        public string Identifier
        {
            get => GetChildByRole(JsTreeRoles.Identifier).Name;
            set => SetChildByRole(JsTreeRoles.Identifier, new JsIdentifier(value));
        }

        public JsIdentifier IdentifierToken
        {
            get => GetChildByRole(JsTreeRoles.Identifier);
            set => SetChildByRole(JsTreeRoles.Identifier, value);
        }

        public JsIdentifierExpression()
        {
        }

        public JsIdentifierExpression(string identifier)
        {
            this.Identifier = identifier;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIdentifierExpression(this);
    }
}
