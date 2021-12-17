using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public sealed class JsAssignmentExpression: JsExpression
    {
        public readonly static JsTreeRole<JsExpression> LeftRole = JsBinaryExpression.LeftRole;
        public readonly static JsTreeRole<JsExpression> RightRole = JsBinaryExpression.RightRole;

        private BinaryOperatorType? @operator;

        public BinaryOperatorType? Operator
        {
            get { return @operator; }
            set { ThrowIfFrozen(); @operator = value; }
        }

        public string OperatorString => (Operator != null ? JsBinaryExpression.GetOperatorString(Operator.Value) : "") + "=";

        public JsExpression Left
        {
            get { return GetChildByRole(LeftRole)!; }
            set { SetChildByRole(LeftRole, value); }
        }

        public JsExpression Right
        {
            get { return GetChildByRole(RightRole)!; }
            set { SetChildByRole(RightRole, value); }
        }

        public JsAssignmentExpression(JsExpression left, JsExpression right, BinaryOperatorType? @operator = null)
        {
            this.@operator = @operator;
            this.Left = left;
            this.Right = right;
        }


        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitAssignmentExpression(this);
    }
}
