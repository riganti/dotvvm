using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace DotVVM.Framework.Runtime.Compilation.JavascriptCompilation
{
    public class JavascriptTranslator
    {

        public static string CompileToJavascript(Expression binding, DataContextStack dataContext)
        {
            var translator = new JavascriptTranslator();
            translator.DataContexts = dataContext;
            var script = translator.Translate(binding).Trim();
            if (binding.NodeType == ExpressionType.MemberAccess && script.EndsWith("()")) script = script.Remove(script.Length - 2);
            return script;
        }

        public static readonly Dictionary<MethodInfo, IJsMethodTranslator> MethodTranslators = new Dictionary<MethodInfo, IJsMethodTranslator>();

        public DataContextStack DataContexts { get; set; }

        public string Translate(Expression expression)
        {
            if (expression is BinaryExpression) return TranslateBinary((BinaryExpression)expression);
            else if (expression is UnaryExpression) return TranslateUnary((UnaryExpression)expression);

            switch (expression.NodeType)
            {
                case ExpressionType.Constant:
                    return TranslateConstant((ConstantExpression)expression);
                case ExpressionType.Call:
                    return TranslateMethodCall((MethodCallExpression)expression);
                case ExpressionType.MemberAccess:
                    return TranslateMemberAccess((MemberExpression)expression);
                case ExpressionType.Parameter:
                    return TranslateParameter((ParameterExpression)expression);
                case ExpressionType.Conditional:
                    return TranslateConditional((ConditionalExpression)expression);
                default:
                    throw new NotSupportedException($"expression type { expression.NodeType } can't be transaled to Javascript");
            }
        }

        public string TranslateConditional(ConditionalExpression expression)
        {
            return $"({ Translate(expression.Test) }) ? ({ Translate(expression.IfTrue) }) : ({ Translate(expression.IfFalse) })";
        }

        public string TranslateParameter(ParameterExpression expression)
        {
            if (expression.Name == "_this") return "$data";
            if (expression.Name == "_parent") return "$parent";
            if (expression.Name == "_root") return "$root";
            if (expression.Name.StartsWith("_parent")) return $"$parents[{ int.Parse(expression.Name.Substring("_parent".Length)) }]";
            if (expression.Name == "_control")
            {
                var c = DataContexts.Parents().Count();
                string context = string.Concat(Enumerable.Repeat("$parentContext.", c));
                return context + "$control";
            }
            throw new NotSupportedException();
        }

        public string TranslateConstant(ConstantExpression expression)
        {
            return JavascriptCompilationHelper.CompileConstant(expression.Value);
        }

        public string TranslateMethodCall(MethodCallExpression expression)
        {
            var thisExpression = Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var result = TryTranslateMethodCall(thisExpression, args, expression.Method);
            if (result == null)
                throw new NotSupportedException($"Method { expression.Method.DeclaringType.Name }.{ expression.Method.Name } can't be translated to Javascript");
            return result;
        }


        protected string TryTranslateMethodCall(string context, string[] args, MethodInfo method)
        {
            IJsMethodTranslator translator;
            if (MethodTranslators.TryGetValue(method, out translator))
            {
                return translator.TranslateCall(context, args, method);
            }
            return null;
        }

        public string TranslateBinary(BinaryExpression expression)
        {
            var left = Translate(expression.Left);
            var right = Translate(expression.Right);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { left, right }, expression.Method);
                if (mTranslate != null) return mTranslate;
            }
            string op = null;
            switch (expression.NodeType)
            {
                case ExpressionType.Equal: op = "=="; break;
                case ExpressionType.NotEqual: op = "!="; break;
                case ExpressionType.AndAlso: op = "&&"; break;
                case ExpressionType.OrElse: op = "||"; break;
                case ExpressionType.GreaterThan: op = ">"; break;
                case ExpressionType.LessThan: op = "<"; break;
                case ExpressionType.GreaterThanOrEqual: op = ">="; break;
                case ExpressionType.LessThanOrEqual: op = "<="; break;
                case ExpressionType.AddChecked:
                case ExpressionType.Add: op = "+"; break;
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddAssign: op = "+="; break;
                case ExpressionType.SubtractChecked:
                case ExpressionType.Subtract: op = "-"; break;
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.SubtractAssign: op = "-="; break;
                case ExpressionType.Divide: op = "/"; break;
                case ExpressionType.DivideAssign: op = "/="; break;
                case ExpressionType.Modulo: op = "%"; break;
                case ExpressionType.ModuloAssign: op = "%="; break;
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Multiply: op = "*"; break;
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.MultiplyAssign: op = "*="; break;
                case ExpressionType.LeftShift: op = "<<"; break;
                case ExpressionType.LeftShiftAssign: op = "<<="; break;
                case ExpressionType.RightShift: op = ">>"; break;
                case ExpressionType.RightShiftAssign: op = ">>="; break;
                case ExpressionType.And: op = "&"; break;
                case ExpressionType.AndAssign: op = "&="; break;
                case ExpressionType.Or: op = "|"; break;
                case ExpressionType.OrAssign: op = "|="; break;
                case ExpressionType.ExclusiveOr: op = "^"; break;
                case ExpressionType.ExclusiveOrAssign: op = "^="; break;
                case ExpressionType.Coalesce: op = "||"; break;

                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            if (!op.Contains('{')) op = "{0}" + op + "{1}";
            return string.Format(op, right, left);
        }

        public string TranslateUnary(UnaryExpression expression)
        {
            var operand = Translate(expression.Operand);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { operand }, expression.Method);
                if (mTranslate != null) return mTranslate;
            }
            string op = null;
            switch (expression.NodeType)
            {
                case ExpressionType.NegateChecked:
                case ExpressionType.Negate:
                    op = "-{0}";
                    break;
                case ExpressionType.UnaryPlus:
                    op = "+{0}";
                    break;
                case ExpressionType.Not:
                    if (expression.Operand.Type == typeof(bool))
                        op = "!{0}";
                    else op = "~{0}";
                    break;
                //case ExpressionType.PreIncrementAssign:
                //    break;
                //case ExpressionType.PreDecrementAssign:
                //    break;
                //case ExpressionType.PostIncrementAssign:
                //    break;
                //case ExpressionType.PostDecrementAssign:
                //    break;
                case ExpressionType.Convert:
                case ExpressionType.TypeAs:
                    // convert does not make sense in Javascript
                    return operand;
                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            return string.Format(op, $"({ operand })");
        }

        public string TranslateMemberAccess(MemberExpression expression)
        {
            return TranslateViewModelProperty(Translate(expression.Expression), expression.Member);
        }

        public string TranslateViewModelProperty(string context, MemberInfo propInfo)
        {
            return context + "." + propInfo.Name + "()";
        }
    }
}
