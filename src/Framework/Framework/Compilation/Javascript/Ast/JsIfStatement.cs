using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsIfStatement: JsStatement
    {
        public JsExpression Condition
        {
            get => GetChildByRole(JsTreeRoles.Condition)!;
            set => SetChildByRole(JsTreeRoles.Condition, value);
        }
        public static JsTreeRole<JsStatement> TrueBranchRole = new JsTreeRole<JsStatement>("TrueBranch");
        public JsStatement TrueBranch
        {
            get => GetChildByRole(TrueBranchRole)!;
            set => SetChildByRole(TrueBranchRole, value);
        }
        public static JsTreeRole<JsStatement> FalseBranchRole = new JsTreeRole<JsStatement>("FalseBranch");
        public JsStatement? FalseBranch
        {
            get => GetChildByRole(FalseBranchRole);
            set => SetChildByRole(FalseBranchRole, value);
        }

        public JsIfStatement(JsExpression condition, JsStatement trueBranch, JsStatement? falseBranch = null)
        {
            this.Condition = condition;
            this.TrueBranch = trueBranch;
            this.FalseBranch = falseBranch;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitIfStatement(this);
    }
}
