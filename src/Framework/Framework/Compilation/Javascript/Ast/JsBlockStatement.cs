using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsBlockStatement : JsStatement
    {
        public static JsTreeRole<JsStatement> BodyRole = new JsTreeRole<JsStatement>("Body");
        public JsNodeCollection<JsStatement> Body => new JsNodeCollection<JsStatement>(this, BodyRole);

        public JsBlockStatement(params JsStatement[] body) : this((IEnumerable<JsStatement>)body) { }
        public JsBlockStatement(IEnumerable<JsStatement> body)
        {
            foreach (var statement in body) AddChild(statement, BodyRole);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitBlockStatement(this);
    }
}
