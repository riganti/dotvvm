using System.Linq;
using DotVVM.Framework.Compilation.Javascript.Ast;

namespace DotVVM.Framework.Compilation.Javascript
{
    /// <summary> Simple iterative "optimizer". Goal is to remove hard to read elements, notably sequence operators and redundant code. </summary>
    public class JsPrettificationVisitor: JsNodeVisitor
    {
        public int Changes { get; private set; } = 0;

        public override void VisitBlockStatement(JsBlockStatement jsBlockStatement)
        {
            base.VisitBlockStatement(jsBlockStatement);
            foreach (var c in jsBlockStatement.Body.ToArray())
            {
                // break down top-level sequence operators
                if (c is JsExpressionStatement { Expression: JsBinaryExpression { Operator: BinaryOperatorType.Sequence }  sequence })
                {
                    jsBlockStatement.Body.InsertBefore(c, sequence.Left.Detach().AsStatement());
                    jsBlockStatement.Body.InsertAfter(c, sequence.Right.Detach().AsStatement());
                    jsBlockStatement.Body.Remove(c);
                    Changes++;
                }

                else if (c is JsReturnStatement { Expression: JsBinaryExpression { Operator: BinaryOperatorType.Sequence } sequenceR } returnStatement)
                {
                    jsBlockStatement.Body.InsertBefore(c, sequenceR.Left.Detach().AsStatement());
                    returnStatement.Expression.ReplaceWith(sequenceR.Right.Detach());
                    Changes++;
                }

                else if (c is JsVariableDefStatement { Initialization: JsBinaryExpression { Operator: BinaryOperatorType.Sequence } sequenceV } variableDef)
                {
                    jsBlockStatement.Body.InsertBefore(c, sequenceV.Left.Detach().AsStatement());
                    variableDef.Initialization.ReplaceWith(sequenceV.Right.Detach());
                    Changes++;
                }
                
                // join variable definition and assignment
                else if (c is JsExpressionStatement { Expression: JsAssignmentExpression { Operator: null, Left: JsIdentifierExpression variable } assignment })
                {
                    // we found expression variable = X
                    // if there is variable def next to it, we can join these

                    var varDef = jsBlockStatement.Body
                        .TakeWhile(v => v != c)
                        .OfType<JsVariableDefStatement>()
                        .FirstOrDefault(v => v.Name == variable.Identifier);

                    if (varDef is object && varDef.Initialization is null)
                    {
                        // there must also be no reference to variable before this expression
                        var expressionsInBetween =
                            jsBlockStatement.Body.SkipWhile(v => v != varDef).Skip(1)
                                                 .TakeWhile(v => v != c)
                                                 .Concat<JsNode>(new [] { assignment.Right });
                        if (!expressionsInBetween
                            .SelectMany(c => c.DescendantsAndSelf)
                            .OfType<JsIdentifier>()
                            .Any(id => id.Name == varDef.Name))
                        {
                            varDef.Remove();
                            c.ReplaceWith(_ => new JsVariableDefStatement(varDef.Name, assignment.Right.Detach()));
                            Changes++;
                        }
                    }
                }
            }
        }

        public override void VisitBinaryExpression(JsBinaryExpression binaryExpression)
        {
            base.VisitBinaryExpression(binaryExpression);
            // when merging attributes or ids, lots of unnecessary pluses are usually there
            if (binaryExpression is { Operator: BinaryOperatorType.Plus, Left: JsLiteral { Value: string leftStr }, Right: JsLiteral { Value: string rightStr } })
            {
                binaryExpression.ReplaceWith(_ => new JsLiteral(leftStr + rightStr));
                Changes++;
            }
            // (X + "a") + "b" -> X + "ab"
            if (binaryExpression is {
                Operator: BinaryOperatorType.Plus,
                Left: JsBinaryExpression {
                    Operator: BinaryOperatorType.Plus,
                    Left: var leftExpr,
                    Right: JsLiteral { Value: string leftStr2 }
                },
                Right: JsLiteral { Value: string rightStr2 } }
            )
            {
                binaryExpression.ReplaceWith(_ => new JsBinaryExpression(
                    leftExpr.Detach(),
                    BinaryOperatorType.Plus,
                    new JsLiteral(leftStr2 + rightStr2)
                ));
                Changes++;
            }

            // "a" + ("b" + X) -> "ab" + X
            if (binaryExpression is {
                Operator: BinaryOperatorType.Plus,
                Left: JsLiteral { Value: string leftStr3 },
                Right: JsBinaryExpression {
                    Operator: BinaryOperatorType.Plus,
                    Left: JsLiteral { Value: string rightStr3 },
                    Right: var rightExpr,
                } })
            {
                binaryExpression.ReplaceWith(_ => new JsBinaryExpression(
                    new JsLiteral(leftStr3 + rightStr3),
                    BinaryOperatorType.Plus,
                    rightExpr.Detach()
                ));
                Changes++;
            }

            // (X + "a") + ("b" + Y) -> X + "ab" + Y
            if (binaryExpression is {
                Operator: BinaryOperatorType.Plus,
                Left: JsBinaryExpression {
                    Operator: BinaryOperatorType.Plus,
                    Left: var leftExpr2,
                    Right: JsLiteral { Value: string leftStr4 },
                },
                Right: JsBinaryExpression {
                    Operator: BinaryOperatorType.Plus,
                    Left: JsLiteral { Value: string rightStr4 },
                    Right: var rightExpr2,
                } })
            {
                binaryExpression.ReplaceWith(_ => new JsBinaryExpression(
                    new JsBinaryExpression(
                        leftExpr2.Detach(),
                        BinaryOperatorType.Plus,
                        new JsLiteral(leftStr4 + rightStr4)
                    ),
                    BinaryOperatorType.Plus,
                    rightExpr2.Detach()
                ));
                Changes++;
            }
        }

        public static void Prettify(JsNode node)
        {
            var v = new JsPrettificationVisitor();

            var iterations = 0;
            do {
                iterations++;
                v.Changes = 0;
                node.AcceptVisitor(v);
            } while (v.Changes > 0 && iterations < 50);
        }
    }
}
