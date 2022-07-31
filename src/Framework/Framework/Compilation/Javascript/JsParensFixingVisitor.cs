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
        public readonly bool IsPreferredSide;

        public OperatorPrecedence(byte precedence, bool isPreferredSide)
        {
            this.Precedence = precedence;
            this.IsPreferredSide = isPreferredSide;
        }

        public bool NeedsParens(byte parentPrecedence)
        {
            // there is an exception to the rule: ?? can not be chained with && and ||, even though it has lower precedence
            // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Nullish_coalescing_operator#no_chaining_with_and_or_or_operators
            if (parentPrecedence == NullishCoalescing && Precedence is ConditionalAnd or ConditionalOr)
                return true;

            return Precedence < parentPrecedence ||
                (Precedence == parentPrecedence & !IsPreferredSide);
        }

        public override string ToString()
        {
            var name = Precedence switch {
                Atomic => "max",
                UnaryPostfix => "postfix unary",
                UnaryPrefix => "prefix unary",
                Multiplication => "*",
                Addition => "+",
                BinaryShifts => ">>",
                Comparison => "<=",
                Equal => "==",
                BitwiseAnd => "&",
                BitwiseXor => "^",
                BitwiseOr => "|",
                ConditionalAnd => "&&",
                ConditionalOr => "||",
                NullishCoalescing => "??",
                Conditional => "? :",
                Assignment => "=",
                ArrowFunction => "() => {}",
                Sequence => ",",
                0 => "0",
                _ => "?"
            };
            return Precedence + (IsPreferredSide ? "+" : "-") + " (" + name + ")";
        }

        public static readonly OperatorPrecedence Max = new OperatorPrecedence(20, true);

        /// <summary> atomic expression, like `x`, `(x + y)`, `0`, `{"f": 123}`, `x[1]`, ... </summary>
        public const byte Atomic = 20;
        /// <summary> postfix unary expressions `x++`, `x--` </summary>
        public const byte UnaryPostfix = 17;
        /// <summary> prefix unary expressions `typeof x`, `!x`, `+x`, `-x`, `++x`, ... </summary>
        public const byte UnaryPrefix = 16;
        /// <summary> Multiplication, division or modulo binary expression </summary>
        public const byte Multiplication = 15;
        /// <summary> Plus or minus binary expression </summary>
        public const byte Addition = 14;
        /// <summary> int32 binary shift expressions `x >> 10`, `x &lt;&lt; 10`, `x >>> 10` </summary>
        public const byte BinaryShifts = 13;
        /// <summary> Comparison expressions `x > 10`, `x &lt; 10`, `x >= 10`, `x in y`, `x instanceof Y` </summary>
        public const byte Comparison = 12;
        /// <summary> expressions `x == 10`, `x != 10`, `x === 10`, `x !== y` </summary>
        public const byte Equal = 11;
        /// <summary> `x &amp; 0xff` </summary>
        public const byte BitwiseAnd = 10;
        /// <summary> `x ^ 0xff` </summary>
        public const byte BitwiseXor = 9;
        /// <summary> `x | 0xff` </summary>
        public const byte BitwiseOr = 8;
        /// <summary> `x == 1 &amp;&amp; y == 1` </summary>
        public const byte ConditionalAnd = 7;
        /// <summary> `x == 1 || y == 1` </summary>
        public const byte ConditionalOr = 6;
        /// <summary> `x ?? y` </summary>
        public const byte NullishCoalescing = 5;
        /// <summary> `x == 1 ? y : z` </summary>
        public const byte Conditional = 4;
        /// <summary> `x = 123` </summary>
        public const byte Assignment = 3;
        /// <summary> `x => x + 1` </summary>
        public const byte ArrowFunction = 2;
        /// <summary> `x, y` </summary>
        public const byte Sequence = 1;
    }

    /// <summary> Wraps nodes with <see cref="JsParenthesizedExpression" /> when needed to preserve semantics when the AST is stringified. </summary>
    public class JsParensFixingVisitor : JsNodeVisitor
    {
        /// <summary> Returns the "inner operator precedence" of the parent node, as seen from the specified child node.
        /// For example: binary expression have the same inner and outer precedence, <see cref="JsParenthesizedExpression" /> will return Atomic (20) from the outside, but 0 from the inside. Similarly, <see cref="JsInvocationExpression" /> is Atomic from the outside, but 1 (Sequence needs parens) from the argument position and 20 (needs to be Atomic) from the invocation target position. </summary>
        public static byte GetParentLevel(JsExpression expression)
        {
            if (expression.Parent is JsParenthesizedExpression or JsReturnStatement or JsExpressionStatement or null)
                return 0;
            if (expression.Role == JsTreeRoles.Argument ||
                expression.Role == JsTreeRoles.Expression && expression.Parent is JsObjectProperty or JsVariableDefStatement)
                return 1;
            return OperatorLevel(expression.Parent as JsExpression);
        }

        /// <summary> Returns the "outer operator precedence" of the <paramref name="expression" />.
        /// For example, Sequence expression will return 1; <see cref="JsInvocationExpression" />, <see cref="JsParenthesizedExpression" />, ... will return Atomic (20). </summary> 
        public static byte OperatorLevel(JsExpression? expression)
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
                    return OperatorPrecedence.Atomic; // 20
                case JsBinaryExpression be:
                    switch (be.Operator) {
                        case BinaryOperatorType.Times:
                        case BinaryOperatorType.Divide:
                        case BinaryOperatorType.Modulo:
                            return OperatorPrecedence.Multiplication; // 15
                        case BinaryOperatorType.Plus:
                        case BinaryOperatorType.Minus:
                            return OperatorPrecedence.Addition; // 14
                        case BinaryOperatorType.LeftShift:
                        case BinaryOperatorType.RightShift:
                        case BinaryOperatorType.UnsignedRightShift:
                            return OperatorPrecedence.BinaryShifts; // 13
                        case BinaryOperatorType.In:
                        case BinaryOperatorType.InstanceOf:
                        case BinaryOperatorType.Greater:
                        case BinaryOperatorType.GreaterOrEqual:
                        case BinaryOperatorType.Less:
                        case BinaryOperatorType.LessOrEqual:
                            return OperatorPrecedence.Comparison; // 12
                        case BinaryOperatorType.Equal:
                        case BinaryOperatorType.NotEqual:
                        case BinaryOperatorType.StrictlyEqual:
                        case BinaryOperatorType.StrictlyNotEqual:
                            return OperatorPrecedence.Equal; // 11
                        case BinaryOperatorType.BitwiseAnd:
                            return OperatorPrecedence.BitwiseAnd; // 10
                        case BinaryOperatorType.BitwiseXOr:
                            return OperatorPrecedence.BitwiseXor; // 9
                        case BinaryOperatorType.BitwiseOr:
                            return OperatorPrecedence.BitwiseOr; // 8
                        case BinaryOperatorType.ConditionalAnd:
                            return OperatorPrecedence.ConditionalAnd; // 7
                        case BinaryOperatorType.ConditionalOr:
                            return OperatorPrecedence.ConditionalOr; // 6
                        case BinaryOperatorType.NullishCoalescing:
                            return OperatorPrecedence.NullishCoalescing; // 5
                        case BinaryOperatorType.Sequence:
                            return OperatorPrecedence.Sequence; // 1
                        default: throw new NotSupportedException();
                    }
                case JsUnaryExpression ue:
                    if (!ue.IsPrefix) return OperatorPrecedence.UnaryPostfix; // 17
                    else return OperatorPrecedence.UnaryPrefix; // 16
                case JsConditionalExpression ce:
                    return OperatorPrecedence.Conditional; // 4
                case JsAssignmentExpression ae:
                    return OperatorPrecedence.Assignment; // 3
                case JsArrowFunctionExpression arrowFunction:
                    return OperatorPrecedence.ArrowFunction; // 2
                case null:
                    return 0;
                default: throw new NotSupportedException();
            }
        }

        /// <summary> Returns true if the expression is in the more associative position of the parent node. For example `a + b` will return true for `a` and return false for `b`. For non-associative nodes (invocations, ...), always returns true.  </summary>
        public static bool IsPreferredSide(JsExpression expression)
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

        /// <summary> Returns the "outer operator precedence" of the specified JS expression. </summary>
        public static OperatorPrecedence GetOperatorPrecedence(JsExpression expression)
        {
            return new OperatorPrecedence(OperatorLevel(expression), IsPreferredSide(expression));
        }

        /// <summary>
        /// Determines if the expression will have to be parenthesized when called from parent expression
        /// </summary>
        public bool NeedsParens(JsExpression expression)
        {
            return GetOperatorPrecedence(expression).NeedsParens(GetParentLevel(expression));
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
