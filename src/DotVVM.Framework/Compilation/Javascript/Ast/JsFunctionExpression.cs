using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsFunctionExpression: JsExpression
    {
        public JsIdentifier Identifier
        {
            get => GetChildByRole(JsTreeRoles.Identifier);
            set => SetChildByRole(JsTreeRoles.Identifier, value);
        }

        public static JsTreeRole<JsIdentifier> ParametersRole = new JsTreeRole<JsIdentifier>("Parameters");
        public JsNodeCollection<JsIdentifier> Parameters => new JsNodeCollection<JsIdentifier>(this, ParametersRole);

        public string IdentifierName
        {
            get => Identifier?.Name;
            set => Identifier = new JsIdentifier(value);
        }

        public static JsTreeRole<JsBlockStatement> BlockRole = new JsTreeRole<JsBlockStatement>("Block");
        public JsBlockStatement Block
        {
            get => GetChildByRole(BlockRole);
            set => SetChildByRole(BlockRole, value);
        }

        public JsFunctionExpression(IEnumerable<JsIdentifier> parameters, JsBlockStatement bodyBlock, JsIdentifier name = null)
        {
            if (name != null) AddChild(name, JsTreeRoles.Identifier);
            foreach (var p in parameters) AddChild(p, ParametersRole);
            AddChild(bodyBlock, BlockRole);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitFunctionExpression(this);

        public static JsExpression CreateIIFE(JsBlockStatement block, IEnumerable<(string name, JsExpression initExpression)> parameters = null)
        {
            if (parameters == null) parameters = Enumerable.Empty<(string, JsExpression)>();
            return new JsFunctionExpression(
                parameters.Select(p => new JsIdentifier(p.name)),
                block
            ).Invoke(parameters.Select(p => p.initExpression));
        }
    }
}
