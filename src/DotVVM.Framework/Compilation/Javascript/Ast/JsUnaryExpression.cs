using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsUnaryExpression : JsExpression
    {
        public JsExpression Expression
        {
            get { return GetChildByRole(JsTreeRoles.Expression); }
            set { SetChildByRole(JsTreeRoles.Expression, value); }
        }

        private UnaryOperatorType @operator;

        public UnaryOperatorType Operator
        {
            get { return @operator; }
            set { ThrowIfFrozen(); @operator = value; }
        }

        public string OperatorString => GetOperatorString(Operator);

        private bool isPrefix;

        public bool IsPrefix
        {
            get { return isPrefix; }
            set { ThrowIfFrozen(); isPrefix = value; }
        }


        public JsUnaryExpression()
        {
        }

        public JsUnaryExpression(UnaryOperatorType op, JsExpression expression, bool isPrefix = true)
        {
            this.Operator = op;
            this.Expression = expression;
            this.IsPrefix = isPrefix;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitUnaryExpression(this);

        public static string GetOperatorString(UnaryOperatorType op)
        {
            switch (op) {
                case UnaryOperatorType.Plus:
                    return "+";
                case UnaryOperatorType.Minus:
                    return "-";
                case UnaryOperatorType.BitwiseNot:
                    return "~";
                case UnaryOperatorType.LogicalNot:
                    return "!";
                case UnaryOperatorType.Delete:
                    return "delete";
                case UnaryOperatorType.Void:
                    return "void";
                case UnaryOperatorType.TypeOf:
                    return "typeof";
                case UnaryOperatorType.Increment:
                    return "++";
                case UnaryOperatorType.Decrement:
                    return "--";
                default:
                    throw new NotSupportedException($"Operator {op} is not supported.");
            }
        }
    }

    public enum UnaryOperatorType
    {
        [EnumMember(Value = "+")]
        Plus,
        [EnumMember(Value = "-")]
        Minus,
        [EnumMember(Value = "~")]
        BitwiseNot,
        [EnumMember(Value = "!")]
        LogicalNot,
        [EnumMember(Value = "delete")]
        Delete,
        [EnumMember(Value = "void")]
        Void,
        [EnumMember(Value = "typeof")]
        TypeOf,
        [EnumMember(Value = "++")]
        Increment,
        [EnumMember(Value = "--")]
        Decrement,
    }
}
