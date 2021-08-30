using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsFunctionExpression: JsBaseFunctionExpression
    {
        public JsIdentifier? Identifier
        {
            get => GetChildByRole(JsTreeRoles.Identifier);
            set => SetChildByRole(JsTreeRoles.Identifier, value);
        }

        public string? IdentifierName
        {
            get => Identifier?.Name;
            set => Identifier = new JsIdentifier(value);
        }

        public JsFunctionExpression(IEnumerable<JsIdentifier> parameters, JsBlockStatement bodyBlock, JsIdentifier? name = null, bool isAsync = false)
        {
            if (name != null) AddChild(name, JsTreeRoles.Identifier);
            foreach (var p in parameters) AddChild(p, ParametersRole);
            AddChild(bodyBlock, BlockRole);
            IsAsync = isAsync;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitFunctionExpression(this);
    }
}
