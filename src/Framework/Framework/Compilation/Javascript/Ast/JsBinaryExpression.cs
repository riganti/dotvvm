using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DotVVM.Framework.Compilation.Javascript.Ast
{
    public class JsBinaryExpression : JsExpression
    {
        public readonly static JsTreeRole<JsExpression> LeftRole = new JsTreeRole<JsExpression>("Left");
        public readonly static JsTreeRole<JsExpression> RightRole = new JsTreeRole<JsExpression>("Right");

        private BinaryOperatorType @operator;

        public BinaryOperatorType Operator
        {
            get { return @operator; }
            set { ThrowIfFrozen(); @operator = value; }
        }

        public string OperatorString => GetOperatorString(Operator);

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

        public JsBinaryExpression(JsExpression left, BinaryOperatorType @operator, JsExpression right)
        {
            this.@operator = @operator;
            this.Left = left;
            this.Right = right;
        }

        public override void AcceptVisitor(IJsNodeVisitor visitor) => visitor.VisitBinaryExpression(this);


        public static string GetOperatorString(BinaryOperatorType op)
        {
            switch (op) {
                case BinaryOperatorType.Plus:
                    return "+";
                case BinaryOperatorType.Minus:
                    return "-";
                case BinaryOperatorType.Times:
                    return "*";
                case BinaryOperatorType.Divide:
                    return "/";
                case BinaryOperatorType.Modulo:
                    return "%";
                case BinaryOperatorType.Equal:
                    return "==";
                case BinaryOperatorType.NotEqual:
                    return "!=";
                case BinaryOperatorType.Greater:
                    return ">";
                case BinaryOperatorType.GreaterOrEqual:
                    return ">=";
                case BinaryOperatorType.Less:
                    return "<";
                case BinaryOperatorType.LessOrEqual:
                    return "<=";
                case BinaryOperatorType.StrictlyEqual:
                    return "===";
                case BinaryOperatorType.StrictlyNotEqual:
                    return "!==";
                case BinaryOperatorType.BitwiseAnd:
                    return "&";
                case BinaryOperatorType.BitwiseOr:
                    return "|";
                case BinaryOperatorType.BitwiseXOr:
                    return "^";
                case BinaryOperatorType.LeftShift:
                    return "<<";
                case BinaryOperatorType.RightShift:
                    return ">>";
                case BinaryOperatorType.UnsignedRightShift:
                    return ">>>";
                case BinaryOperatorType.InstanceOf:
                    return "instanceof";
                case BinaryOperatorType.In:
                    return "in";
                case BinaryOperatorType.ConditionalAnd:
                    return "&&";
                case BinaryOperatorType.ConditionalOr:
                    return "||";
                case BinaryOperatorType.Assignment:
                    return "=";
                case BinaryOperatorType.Sequence:
                    return ",";
                default:
                    throw new NotSupportedException($"Operator {op} not supported.");
            }
        }
    }

    public enum BinaryOperatorType
    {
        [EnumMember(Value = "+")]
        Plus,
        [EnumMember(Value = "-")]
        Minus,
        [EnumMember(Value = "*")]
        Times,
        [EnumMember(Value = "/")]
        Divide,
        [EnumMember(Value = "%")]
        Modulo,
        [EnumMember(Value = "==")]
        Equal,
        [EnumMember(Value = "!=")]
        NotEqual,
        [EnumMember(Value = ">")]
        Greater,
        [EnumMember(Value = ">=")]
        GreaterOrEqual,
        [EnumMember(Value = "<")]
        Less,
        [EnumMember(Value = "<=")]
        LessOrEqual,
        [EnumMember(Value = "===")]
        StrictlyEqual,
        [EnumMember(Value = "!==")]
        StrictlyNotEqual,
        [EnumMember(Value = "&")]
        BitwiseAnd,
        [EnumMember(Value = "|")]
        BitwiseOr,
        [EnumMember(Value = "^")]
        BitwiseXOr,
        [EnumMember(Value = "<<")]
        LeftShift,
        [EnumMember(Value = ">>")]
        RightShift,
        [EnumMember(Value = ">>>")]
        UnsignedRightShift,
        [EnumMember(Value = "instanceof")]
        InstanceOf,
        [EnumMember(Value = "in")]
        In,
        [EnumMember(Value = "&&")]
        ConditionalAnd,
        [EnumMember(Value = "||")]
        ConditionalOr,
        [EnumMember(Value = "=")]
        Assignment,
        [EnumMember(Value = ",")]
        Sequence
    }
}
