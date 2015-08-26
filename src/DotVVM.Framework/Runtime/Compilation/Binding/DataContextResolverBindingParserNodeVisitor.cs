using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Parser.Binding.Parser;
using DotVVM.Framework.Parser.Binding.Tokenizer;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class DataContextResolverBindingParserNodeVisitor : BindingParserNodeVisitor<bool>
    {

        public DataContextStack DataContextStack { get; set; }


        protected override bool VisitArrayAccess(ArrayAccessBindingParserNode node)
        {
            node.TargetExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = CompileTimeTypeConstraint.Array,
                DataContextStack = GetContext(node).DataContextStack
            };
            Visit(node.TargetExpression);

            node.ArrayIndexExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = CompileTimeTypeConstraint.ExactType(typeof(int)),
                DataContextStack = GetContext(node).DataContextStack
            };
            Visit(node.ArrayIndexExpression);

            var actualType = GetContext(node.TargetExpression).ActualType;
            GetContext(node).ActualType = GetArrayElementType(actualType);

            return true;
        }


        protected override bool VisitBinaryOperator(BinaryOperatorBindingParserNode node)
        {
            CompileTimeTypeConstraint constraint;

            if (node.Operator == BindingTokenType.AddOperator || node.Operator == BindingTokenType.SubtractOperator 
                || node.Operator == BindingTokenType.MultiplyOperator || node.Operator == BindingTokenType.DivideOperator
                || node.Operator == BindingTokenType.ModulusOperator 
                || node.Operator == BindingTokenType.GreaterThanOperator || node.Operator == BindingTokenType.GreaterThanEqualsOperator 
                || node.Operator == BindingTokenType.LessThanOperator || node.Operator == BindingTokenType.LessThanEqualsOperator)
            {
                constraint = CompileTimeTypeConstraint.Numeric;
            }
            else if (node.Operator == BindingTokenType.EqualsEqualsOperator || node.Operator == BindingTokenType.NotEqualsOperator)
            {
                constraint = CompileTimeTypeConstraint.AnyOf(CompileTimeTypeConstraint.Any);
            }
            else if (node.Operator == BindingTokenType.AndAlsoOperator || node.Operator == BindingTokenType.OrElseOperator)
            {
                constraint = CompileTimeTypeConstraint.Boolean;
            }
            else if (node.Operator == BindingTokenType.AndOperator || node.Operator == BindingTokenType.OrOperator)
            {
                constraint = CompileTimeTypeConstraint.AnyOf(CompileTimeTypeConstraint.Numeric, CompileTimeTypeConstraint.Boolean);
            }
            else if (node.Operator == BindingTokenType.NullCoalescingOperator)
            {
                constraint = CompileTimeTypeConstraint.AnyOf(CompileTimeTypeConstraint.Any);
            }
            else
            {
                return base.VisitBinaryOperator(node);
            }

            node.FirstExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = constraint,
                DataContextStack = GetContext(node).DataContextStack
            };
            Visit(node.FirstExpression);

            node.SecondExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = constraint,
                DataContextStack = GetContext(node).DataContextStack
            };
            Visit(node.SecondExpression);

            // TODO: find common type

            return true;
        }

        protected override bool VisitConditionalExpression(ConditionalExpressionBindingParserNode node)
        {
            node.ConditionExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = CompileTimeTypeConstraint.Boolean,
                DataContextStack = GetContext(node).DataContextStack
            };
            node.TrueExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = CompileTimeTypeConstraint.Any,
                DataContextStack = GetContext(node).DataContextStack
            };
            node.FalseExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = CompileTimeTypeConstraint.Any,
                DataContextStack = GetContext(node).DataContextStack
            };

            Visit(node.ConditionExpression);
            Visit(node.TrueExpression);
            Visit(node.FalseExpression);

            // find common base type
            var type1 = GetContext(node.TrueExpression).ActualType;
            var type2 = GetContext(node.FalseExpression).ActualType;

            // TODO:

            return true;
        }

        protected override bool VisitFunctionCall(FunctionCallBindingParserNode node)
        {
            return base.VisitFunctionCall(node);
        }

        protected override bool VisitIdentifierName(IdentifierNameBindingParserNode node)
        {
            return base.VisitIdentifierName(node);
        }

        protected override bool VisitLiteralExpression(LiteralExpressionBindingParserNode node)
        {
            GetContext(node).ActualType = node.Value?.GetType() ?? typeof (object);
            return true;
        }

        protected override bool VisitMemberAccess(MemberAccessBindingParserNode node)
        {
            return base.VisitMemberAccess(node);
        }

        protected override bool VisitParenthesizedExpression(ParenthesizedExpressionBindingParserNode node)
        {
            node.InnerExpression.Context = new CompileTimeParserNodeContext()
            {
                DesiredType = GetContext(node).DesiredType,
                DataContextStack = GetContext(node).DataContextStack
            };
            Visit(node.InnerExpression);
            GetContext(node).ActualType = GetContext(node.InnerExpression).ActualType;
            return true;
        }

        protected override bool VisitSpecialProperty(SpecialPropertyBindingParserNode node)
        {
            if (node.SpecialProperty == BindingSpecialProperty.This)
            {
                GetContext(node).ActualType = GetContext(node).DataContextStack.DataContextType;
                return true;
            }
            else if (node.SpecialProperty == BindingSpecialProperty.Parent)
            {
                GetContext(node).ActualType = GetContext(node).DataContextStack.Parent.DataContextType;
                return true;
            }
            else if (node.SpecialProperty == BindingSpecialProperty.Root)
            {
                GetContext(node).ActualType = GetContext(node).DataContextStack.Parents().Last();
                return true;
            }

            return base.VisitSpecialProperty(node);
        }

        protected override bool VisitUnaryOperator(UnaryOperatorBindingParserNode node)
        {
            if (node.Operator == BindingTokenType.NotOperator)
            {
                node.InnerExpression.Context = new CompileTimeParserNodeContext()
                {
                    DesiredType = CompileTimeTypeConstraint.Boolean,
                    DataContextStack = GetContext(node).DataContextStack
                };
                Visit(node.InnerExpression);
                GetContext(node).ActualType = GetContext(node.InnerExpression).ActualType;
                return true;
            }
            else if (node.Operator == BindingTokenType.SubtractOperator)
            {
                node.InnerExpression.Context = new CompileTimeParserNodeContext()
                {
                    DesiredType = CompileTimeTypeConstraint.Numeric,
                    DataContextStack = GetContext(node).DataContextStack
                };
                Visit(node.InnerExpression);
                GetContext(node).ActualType = GetContext(node.InnerExpression).ActualType;
                return true;
            }
            else
            {
                return base.VisitUnaryOperator(node);
            }
        }

        private CompileTimeParserNodeContext GetContext(BindingParserNode node)
        {
            return (CompileTimeParserNodeContext) node.Context;
        }



        private Type GetArrayElementType(Type actualType)
        {
            if (actualType == null) return null;

            if (actualType.IsArray)
            {
                return actualType.GetElementType();
            }

            var typedList = actualType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IList<>));
            if (typedList != null)
            {
                return typedList.GetGenericArguments()[0];
            }
            else if (actualType.IsInstanceOfType(typeof (IList)))
            {
                return typeof (object);
            }

            return null;
        }
    }
}