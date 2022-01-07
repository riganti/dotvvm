using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsVariableDefStatement : JsStatement
    {
        private string keyword = "let";
        public string Keyword
        {
            get { return keyword; }
            set { ThrowIfFrozen(); keyword = value; }
        }

        public JsIdentifier NameIdentifier
        {
            get { return GetChildByRole(JsTreeRoles.Identifier).NotNull(); }
            set { SetChildByRole(JsTreeRoles.Identifier, value.NotNull()); }
        }

        public string Name
        {
            get { return NameIdentifier.Name; }
            set { NameIdentifier = new JsIdentifier(value); }
        }
        

        public JsExpression? Initialization
        {
            get { return GetChildByRole(JsTreeRoles.Expression); }
            set { SetChildByRole(JsTreeRoles.Expression, value); }
        }

        public JsVariableDefStatement(string name, JsExpression? expr = null)
        {
            this.Name = name;
            this.Initialization = expr;
        }


        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitVariableDefStatement(this);
    }
}
