using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Resources;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Framework.Binding
{
    public class ExpressionEvaluationVisitor : CSharpSyntaxVisitor<object>
    {

        /// <summary>
        /// Gets or sets a value whether the evaluator can return a MethodInfo.
        /// </summary>
        public bool AllowMethods { get; set; }

        /// <summary>
        /// The hierarchy of object hierarchy relative to the root at the time when expression evaluation finished.
        /// </summary>
        public Stack<object> Hierarchy { get; private set; }

        /// <summary>
        /// The hierarchy of DataContext bindings relative to the root at the time when expression evaluation finished.
        /// </summary>
        public Stack<string> PathHierarchy { get; private set; }

        /// <summary>
        /// Gets or sets the view root control.
        /// </summary>
        public DotvvmControl ViewRootControl { get; private set; }

        /// <summary>
        /// Gets or sets the target object on which the returned MethodInfo should be invoked.
        /// </summary>
        public object MethodInvocationTarget { get; private set; }

        /// <summary>
        /// Gets the target on which last property was evaluated.
        /// </summary>
        public object Result
        {
            get { return Hierarchy.Peek(); }
        }

        /// <summary>
        /// Backups the current position in the view model.
        /// </summary>
        public void BackupCurrentPosition(object result, string path)
        {
            Hierarchy.Push(result);
            PathHierarchy.Push(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionEvaluationVisitor"/> class.
        /// </summary>
        public ExpressionEvaluationVisitor(object root, DotvvmControl viewRootControl, List<object> hierarchy = null)
        {
            ViewRootControl = viewRootControl;
            PathHierarchy = new Stack<string>();

            if (hierarchy == null)
            {
                Hierarchy = new Stack<object>();
                Hierarchy.Push(root);
            }
            else
            {
                Hierarchy = new Stack<object>(hierarchy);
            }
        }

        /// <summary>
        /// Visits the expression statement.
        /// </summary>
        public override object VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the name of the identifier.
        /// </summary>
        public override object VisitIdentifierName(IdentifierNameSyntax node)
        {
            return GetObjectProperty(Result, node.ToString());
        }

        /// <summary>
        /// Visits the element access expression.
        /// </summary>
        public override object VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            var value = Visit(node.Expression);

            var array = value as IList;
            if (array == null && value is IGridViewDataSet)
            {
                // GridViewDataSet has special handling - the .Items in the command path is optional
                array = ((IGridViewDataSet)value).Items;
            }

            if (array == null) return null;

            if (node.ArgumentList.Arguments.Count == 1)
            {
                var index = Visit(node.ArgumentList.Arguments.First()) as int?;
                if (index != null)
                {
                    return array[index.Value];
                }
            }

            return base.VisitElementAccessExpression(node);
        }
        

        /// <summary>
        /// Visits the argument.
        /// </summary>
        public override object VisitArgument(ArgumentSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the literal expression.
        /// </summary>
        public override object VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                return node.Token.Value as int?;
            }
            else if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return node.Token.Value as string;
            }
            else if (node.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return null;
            }
            else if (node.IsKind(SyntaxKind.TrueLiteralExpression))
            {
                return true;
            }
            else if (node.IsKind(SyntaxKind.FalseLiteralExpression))
            {
                return false;
            }

            return base.VisitLiteralExpression(node);
        }

        /// <summary>
        /// Visits the parenthesized expression.
        /// </summary>
        public override object VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return Visit(node.Expression);
        }

        /// <summary>
        /// Visits the member access expression.
        /// </summary>
        public override object VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            return GetObjectProperty(Visit(node.Expression), node.Name.ToString());
        }

        /// <summary>
        /// Gets the specified property of a given object.
        /// </summary>
        private object GetObjectProperty(object target, string propertyName)
        {
            if (target == null) return null;

            if (propertyName == Constants.ParentSpecialBindingProperty)
            {
                Hierarchy.Pop();
                if (PathHierarchy.Any())
                    PathHierarchy.Pop();
                return Result;
            }
            else if (propertyName == Constants.RootSpecialBindingProperty)
            {
                while (Hierarchy.Count > 1)
                {
                    Hierarchy.Pop();
                    if (PathHierarchy.Any())
                        PathHierarchy.Pop();
                }
                return Result;
            }
            else if (propertyName == Constants.ThisSpecialBindingProperty)
            {
                return target;
            }
            else if (propertyName.StartsWith(Constants.ControlStateSpecialBindingProperty))
            {
                return GetControlState(propertyName.Substring(Constants.ControlStateSpecialBindingProperty.Length));
            }
            else if (target is IDictionary<string, object>)
            {
                var dict = ((IDictionary<string, object>)target);
                object value;
                dict.TryGetValue(propertyName, out value);
                return value;
            }
            else
            {
                var property = target.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    // evaluate property
                    return property.GetValue(target);
                }
                else if (AllowMethods)
                {
                    // evaluate method
                    var methods = target.GetType().GetMethods().Where(n => n.Name == propertyName).ToList();
                    if (methods.Count == 0)
                    {
                        throw new ParserException(string.Format("The method {0} was not found on type {1}!", propertyName, target.GetType()));
                    }
                    else if (methods.Count > 1)
                    {
                        throw new ParserException(string.Format("The method {0} on type {1} is overloaded which is not supported yet!", propertyName, target.GetType()));
                    }
                    MethodInvocationTarget = target;
                    return methods[0];
                }
                else
                {
                    throw new ParserException(string.Format("The property {0} was not found on type {1}!", propertyName, target.GetType()));
                }
            }
        }

        /// <summary>
        /// Visits the prefix unary expression.
        /// </summary>
        public override object VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
        {
            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return NumberUtils.Negate(Visit(node.Operand));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
            {
                return !(Visit(node.Operand) as bool?);
            }

            return base.VisitPrefixUnaryExpression(node);
        }

        private static void ThrowArithmeticOperatorsNotSupportedYet()
        {
            throw new NotSupportedException("Server evaluation of numeric expressions is not supported yet!");
        }

        /// <summary>
        /// Visits the binary expression.
        /// </summary>
        public override object VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            // arithmetic
            if (node.OperatorToken.IsKind(SyntaxKind.PlusToken))
            {
                return NumberUtils.Add(Visit(node.Left), Visit(node.Right));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.MinusToken))
            {
                return NumberUtils.Subtract(Visit(node.Left), Visit(node.Right));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.AsteriskToken))
            {
                return NumberUtils.Multiply(Visit(node.Left), Visit(node.Right));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.SlashToken))
            {
                return NumberUtils.Divide(Visit(node.Left), Visit(node.Right));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.PercentToken))
            {
                return NumberUtils.Mod(Visit(node.Left), Visit(node.Right));
            }

            // comparison
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanToken))
            {
                ThrowArithmeticOperatorsNotSupportedYet();
            }
            if (node.OperatorToken.IsKind(SyntaxKind.LessThanEqualsToken))
            {
                ThrowArithmeticOperatorsNotSupportedYet();
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanToken))
            {
                ThrowArithmeticOperatorsNotSupportedYet();
            }
            if (node.OperatorToken.IsKind(SyntaxKind.GreaterThanEqualsToken))
            {
                ThrowArithmeticOperatorsNotSupportedYet();
            }
            if (node.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken))
            {
                return Equals(Visit(node.Left), Visit(node.Right));
            }
            if (node.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
            {
                return !Equals(Visit(node.Left), Visit(node.Right));
            }

            // null coalescing operator
            if (node.OperatorToken.IsKind(SyntaxKind.QuestionQuestionToken))
            {
                return Visit(node.Left) ?? Visit(node.Right);
            }

            return base.VisitBinaryExpression(node);
        }

        /// <summary>
        /// Finds the control state for current DataContext hierarchy.
        /// </summary>
        private Dictionary<string, object> GetControlState(string controlId)
        {
            Dictionary<string, object> result = null;

            var treeWalker = new ControlTreeWalker(ViewRootControl);
            treeWalker.ProcessControlTree((control) =>
            {
                // if the hierarchies are equal, return the control
                if (control is DotvvmBindableControl && ((DotvvmBindableControl)control).RequiresControlState)
                {
                    if (ComparePathHierarchies(treeWalker.CurrentPath, PathHierarchy))
                    {
                        control.EnsureControlHasId();
                        if (control.ID == controlId)
                        {
                            result = ((DotvvmBindableControl)control).ControlState;
                        }
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Compares the path hierarchies.
        /// </summary>
        internal bool ComparePathHierarchies(Stack<string> hierarchy1, Stack<string> hierarchy2)
        {
            if (hierarchy1.Count == hierarchy2.Count)
            {
                var enumerator1 = hierarchy1.GetEnumerator();
                var enumerator2 = hierarchy2.GetEnumerator();
                while (enumerator1.MoveNext())
                {
                    enumerator2.MoveNext();
                    if (enumerator1.Current != enumerator2.Current)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override object DefaultVisit(SyntaxNode node)
        {
            throw new ParserException(string.Format(Parser_Dothtml.Binding_UnsupportedExpression, node));
        }


        /// <summary>
        /// Determines whether the specified property name is a special property in the data binding expressions.
        /// </summary>
        public bool IsSpecialPropertyName(string propertyName)
        {
            return propertyName == Constants.RootSpecialBindingProperty || propertyName == Constants.ParentSpecialBindingProperty
                || propertyName == Constants.ThisSpecialBindingProperty || propertyName.StartsWith(Constants.ControlStateSpecialBindingProperty);
        }

    }
}