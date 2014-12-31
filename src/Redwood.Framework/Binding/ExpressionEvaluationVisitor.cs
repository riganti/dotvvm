using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.Resources;
using Redwood.Framework.ViewModel;

namespace Redwood.Framework.Binding
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
        public RedwoodControl ViewRootControl { get; private set; }


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
        public ExpressionEvaluationVisitor(object root, RedwoodControl viewRootControl, List<object> hierarchy = null)
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
            var array = Visit(node.Expression) as IList;
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
                    return methods[0];
                }
                else
                {
                    throw new ParserException(string.Format("The property {0} was not found on type {1}!", propertyName, target.GetType()));
                }
            }
        }

        /// <summary>
        /// Finds the control state for current DataContext hierarchy.
        /// </summary>
        private Dictionary<string, object> GetControlState(string controlId)
        {
            Dictionary<string, object> result = null;

            var treeWalker = new NonEvaluatingControlTreeWalker(ViewRootControl);
            treeWalker.ProcessControlTree((viewModel, control) =>
            {
                // if the hierarchies are equal, return the control
                if (control is RedwoodBindableControl && ((RedwoodBindableControl)control).RequiresControlState)
                {
                    if (ComparePathHierarchies(treeWalker.CurrentPath, PathHierarchy))
                    {
                        control.EnsureControlHasId();
                        if (control.ID == controlId)
                        {
                            result = ((RedwoodBindableControl)control).ControlState;
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
            throw new ParserException(string.Format(Parser_RwHtml.Binding_UnsupportedExpression, node));
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