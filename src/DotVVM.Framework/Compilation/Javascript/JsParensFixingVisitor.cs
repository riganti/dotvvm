using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JsParensFixingVisitor: JsNodeVisitor
    {
        /// <summary>
        /// Determines if the expression will have to be parenthised when called from parent expression
        /// </summary>
        public bool NeedsParens(JsExpression expression)
        {
            int level(JsExpression e)
            {
                switch (e) {
                    case JsParenthesizedExpression _:
                    case JsMemberAccessExpression _:
                    case JsInvocationExpression _:
                    case JsIndexerExpression _:
                    case JsIdentifierExpression _:
                    case JsLiteral _:
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
                            default: throw new NotSupportedException();
                        }
                    case JsUnaryExpression ue:
                        if (!ue.IsPrefix) return 17;
                        else return 16;
                    case JsConditionalExpression ce:
                        return 4;
                    case JsAssignmentExpression ae:
                        return 3;
                    default: throw new NotSupportedException();
                }
            }
            if (expression.Role == JsTreeRoles.Argument || expression.Parent is JsParenthesizedExpression || expression.Parent is null) return false; // argument is never parenthised
            var level0 = level(expression);
            var level1 = level(expression.Parent as JsExpression);
            if (level0 > level1) return false;
            else if (level0 < level1) return true;
            else if (expression is JsBinaryExpression || expression is JsAssignmentExpression) {
                // asociativity..., it is important to avoid common string concat patterns (((a+b)+c)+d)+e). And JS + is not asociative - (""+3+5)==="35" vs (""+(3+5))==="8"
                // all binary operators are currently left-to-right - a+b+c === (a+b)+c
                // include parens when the expression is on the right side
                return expression.Role == JsBinaryExpression.RightRole;
            }
            else if (expression is JsConditionalExpression) {
                // these are right-to-left asociative
                return expression.Role == JsConditionalExpression.ConditionRole;
            }
            return false;
        }

        protected override void DefaultVisit(JsNode node)
        {
            if (node is JsExpression expression && NeedsParens(expression)) {
                expression.ReplaceWith(_ => new JsParenthesizedExpression(expression));
            }
            base.DefaultVisit(node);
        }
    }

    public static class ParensFixingHelper
    {
    }
}