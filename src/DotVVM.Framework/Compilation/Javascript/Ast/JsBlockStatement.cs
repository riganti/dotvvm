using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsBlockStatement : JsStatement
    {
        public static JsTreeRole<JsStatement> BodyRole = new JsTreeRole<JsStatement>("Body");
        public JsNodeCollection<JsStatement> Body => new JsNodeCollection<JsStatement>(this, BodyRole);

        public JsBlockStatement(params JsStatement[] statements) : this((IEnumerable<JsStatement>)statements) { }
        public JsBlockStatement(IEnumerable<JsStatement> statements)
        {
            foreach (var statement in statements) AddChild(statement, BodyRole);
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitBlockStatement(this);
    }
}
