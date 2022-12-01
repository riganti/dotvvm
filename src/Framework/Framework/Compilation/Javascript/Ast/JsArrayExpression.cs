using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsArrayExpression : JsExpression
    {
        public JsNodeCollection<JsExpression> Arguments => new JsNodeCollection<JsExpression>(this, JsTreeRoles.Argument);

        public JsArrayExpression(params JsExpression[] arguments) : this((IEnumerable<JsExpression>)arguments) { }
        public JsArrayExpression(IEnumerable<JsExpression> arguments)
        {
            foreach (var a in arguments) AddChild(a, JsTreeRoles.Argument);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitArrayExpression(this);
    }
}
