using DotVVM.Framework.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.BindingExpressionTree
{
    public class BindingExpressionBuildingVisitor : ExpressionVisitor
    {
        public BindingExpressionNode Expression => values.FirstOrDefault();

        private List<BindingExpressionNode> values = new List<BindingExpressionNode>();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var svalues = values;
            values = new List<BindingExpressionNode>();
            Visit(node.Object);
            var obj = values.FirstOrDefault();
            values.Clear();
            Visit(node.Arguments);
            var args = values;
            values = svalues;
            values.Add(new MethodInvocation(obj, node.Method, args.ToArray()));
            return null;
        }

        protected override Expression VisitConstant(System.Linq.Expressions.ConstantExpression node)
        {
            values.Add(new ConstantExpression(node.Value));
            return base.VisitConstant(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var svalues = values;
            values = new List<BindingExpressionNode>();
            Visit(node.Expression);
            var expr = values.Single();
            values = svalues;
            if (node.Member.MemberType == MemberTypes.Property)
            {
                if (expr.IsViewModel)
                {
                    values.Add(new ViewModelPropertyAccess(expr, (PropertyInfo)node.Member));
                }
                else
                {
                    values.Add(new MethodInvocation(expr, ((PropertyInfo)node.Member).SetMethod));
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            return null;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var name = node.Name;
            var type = KeywordType.This;
            if (name == Constants.ThisSpecialBindingProperty || name == "scope")
                type = KeywordType.This;
            else if (name == Constants.RootSpecialBindingProperty)
                type = KeywordType.Root;
            else if (name.StartsWith(Constants.ParentSpecialBindingProperty))
            {
                var number = name.Substring(Constants.ParentSpecialBindingProperty.Length);
                if (string.IsNullOrEmpty(number))
                    type = KeywordType.Parent;
                else type = KeywordType.Parent + int.Parse(number);
            }
            else throw new NotSupportedException();
            values.Add(new KeywordExpression(type, node.Type));

            return base.VisitParameter(node);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var svalues = values;
            values = new List<BindingExpressionNode>();
            Visit(node.Test);
            Visit(node.IfTrue);
            Visit(node.IfFalse);

            if (values.Count != 3) throw new Exception();

            svalues.Add(BasicMethods.CreateIIF(node.Type, values[0], values[1], values[2]));

            values = svalues;
            return null;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var svalues = values;
            values = new List<BindingExpressionNode>();
            Visit(node.Operand);
            if (node.Method == null)
            {
                if (node.Type == typeof(bool) && node.NodeType == ExpressionType.Not)
                {
                    var method = typeof(BasicMethods).GetMethod("Not");
                    svalues.Add(new MethodInvocation(null, method, values.Single()));
                }
                else throw new NotSupportedException("unary operator not supported");

            }
            else
            {
                svalues.Add(new MethodInvocation(null, node.Method, values.Single()));
            }

            values = svalues;
            return null;
        }
    }
}
