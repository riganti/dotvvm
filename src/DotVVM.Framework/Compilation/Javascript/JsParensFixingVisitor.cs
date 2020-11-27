using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public readonly struct OperatorPrecedence
    {
        public readonly byte Precedence;
        public readonly bool IsPreferedSide;

        public OperatorPrecedence(byte precedence, bool isPreferedSide)
        {
            this.Precedence = precedence;
            this.IsPreferedSide = isPreferedSide;
        }

        public bool NeedsParens(byte parentPrecedence)
        {
            return Precedence < parentPrecedence ||
                (Precedence == parentPrecedence & !IsPreferedSide);
        }

        public override string ToString()
        {
            var name = Precedence switch {
                20 => "max",
                17 => "postfix unary",
                16 => "prefix unary",
                14 => "*",
                13 => "+",
                12 => ">>",
                11 => "<=",
                10 => "==",
                9 => "&&",
                8 => "^",
                7 => "|",
                6 => "&&",
                5 => "||",
                4 => "? :",
                3 => "=",
                0 => ",",
                _ => "?"
            };
            return Precedence + (IsPreferedSide ? "+" : "-") + " (" + name + ")";
        }

        public static readonly OperatorPrecedence Max = new OperatorPrecedence(20, true);
    }

    public class JsParensFixingVisitor : JsNodeVisitor
    {
        public static byte OperatorLevel(JsExpression expression)
        {
            switch (expression) {
                case JsParenthesizedExpression _:
                case JsMemberAccessExpression _:
                case JsInvocationExpression _:
                case JsIndexerExpression _:
                case JsIdentifierExpression _:
                case JsLiteral _:
                case JsSymbolicParameter _:
                case JsFunctionExpression _:
                case JsObjectExpression _:
                case JsArrayExpression _:
                case JsNewExpression _:
                    return 20;
                case JsBinaryExpression be:
                    switch (be.Operator) {
                        case BinaryOperatorType.Times:
                        case BinaryOperatorType.Divide:
                        case BinaryOperatorType.Modulo:
                            return 14;
                        case BinaryOperatorType.Plus:
                        case BinaryOperatorType.Minus:
                            return 13;
                        case BinaryOperatorType.LeftShift:
                        case BinaryOperatorType.RightShift:
                        case BinaryOperatorType.UnsignedRightShift:
                            return 12;
                        case BinaryOperatorType.In:
                        case BinaryOperatorType.InstanceOf:
                        case BinaryOperatorType.Greater:
                        case BinaryOperatorType.GreaterOrEqual:
                        case BinaryOperatorType.Less:
                        case BinaryOperatorType.LessOrEqual:
                            return 11;
                        case BinaryOperatorType.Equal:
                        case BinaryOperatorType.NotEqual:
                        case BinaryOperatorType.StrictlyEqual:
                        case BinaryOperatorType.StricltyNotEqual:
                            return 10;
                        case BinaryOperatorType.BitwiseAnd:
                            return 9;
                        case BinaryOperatorType.BitwiseXOr:
                            return 8;
                        case BinaryOperatorType.BitwiseOr:
                            return 7;
                        case BinaryOperatorType.ConditionalAnd:
                            return 6;
                        case BinaryOperatorType.ConditionalOr:
                            return 5;
                        case BinaryOperatorType.Sequence:
                            return 0;
                        default: throw new NotSupportedException();
                    }
                case JsUnaryExpression ue:
                    if (!ue.IsPrefix) return 17;
                    else return 16;
                case JsConditionalExpression ce:
                    return 4;
                case JsAssignmentExpression ae:
                    return 3;
                case null:
                    return 0;
                default: throw new NotSupportedException();
            }
        }

        public static bool IsPreferedSide(JsExpression expression)
        {
            switch (expression)
            {
                case JsBinaryExpression _:
                    // associativity, it is important to avoid common string concat patterns (((a+b)+c)+d)+e). Be aware JS + is not associative - (""+3+5)==="35" vs (""+(3+5))==="8"
                    // all binary operators are currently left-to-right - a+b+c === (a+b)+c
                    // include parens when the expression is on the right side
                    return expression.Role == JsBinaryExpression.LeftRole;
                case JsAssignmentExpression _:
                    return expression.Role == JsAssignmentExpression.RightRole;
                case JsConditionalExpression _:
                    // these are right-to-left associative
                    return expression.Role != JsConditionalExpression.ConditionRole;
                default:
                    return true;
            }
        }

        public static OperatorPrecedence GetOperatorPrecedence(JsExpression expression)
        {
            if (expression.Role == JsTreeRoles.Argument && expression is JsBinaryExpression binary && binary.Operator == BinaryOperatorType.Sequence)
                return new OperatorPrecedence(1, true);
            if (expression.Role == JsTreeRoles.Argument || expression.Parent is JsParenthesizedExpression)
                return OperatorPrecedence.Max;
            return new OperatorPrecedence(OperatorLevel(expression), IsPreferedSide(expression));
        }

        /// <summary>
        /// Determines if the expression will have to be parenthesized when called from parent expression
        /// </summary>
        public bool NeedsParens(JsExpression expression)
        {
            return GetOperatorPrecedence(expression).NeedsParens(OperatorLevel(expression.Parent as JsExpression));
        }

        protected override void DefaultVisit(JsNode node)
        {
            if (node is JsExpression expression && NeedsParens(expression)) {
                expression.ReplaceWith(_ => new JsParenthesizedExpression(expression));
            }
            base.DefaultVisit(node);
        }
    }
}
