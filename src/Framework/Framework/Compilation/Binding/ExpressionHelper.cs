using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Microsoft.CSharp.RuntimeBinder;

namespace DotVVM.Framework.Compilation.Binding
{
    public static class ExpressionHelper
    {

        public static Expression RewriteTaskSequence(Expression left, Expression right)
        {
            // if the left side is a task, make the right side also a task and join them
            Expression rightTask = WrapAsTask(right);

            // join the tasks using CommandTaskSequenceHelper
            if (rightTask.Type.IsGenericType)
            {
                return Expression.Call(typeof(CommandTaskSequenceHelper), nameof(CommandTaskSequenceHelper.JoinTasks), new[] { rightTask.Type.GetGenericArguments()[0] }, left, Expression.Lambda(rightTask));
            }
            else
            {
                return Expression.Call(typeof(CommandTaskSequenceHelper), nameof(CommandTaskSequenceHelper.JoinTasks), Type.EmptyTypes, left, Expression.Lambda(rightTask));
            }
        }

        public static Expression WrapAsTask(Expression expr)
        {
            if (typeof(Task).IsAssignableFrom(expr.Type))
                return expr;
            if (expr.Type != typeof(void))
                return Expression.Call(typeof(Task), "FromResult", new [] { expr.Type }, expr);
            else
                return Expression.Block(
                    expr,
                    ExpressionUtils.Replace(() => Task.CompletedTask)
                );

        }

        public static Expression UnwrapNullable(this Expression expression) =>
            expression.Type.IsNullable() ? Expression.Property(expression, "Value") : expression;

        public static Expression GetIndexer(Expression expr, Expression index)
        {
            if (expr.Type.IsArray) return Expression.ArrayIndex(expr, index);

            var indexProp = (from p in expr.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                             let param = p.GetIndexParameters()
                             where param.Length == 1
                             let indexConvert = TypeConversion.ImplicitConversion(index, param[0].ParameterType)
                             where indexConvert != null
                             select Expression.MakeIndex(expr, p, new[] { indexConvert })).ToArray();
            if (indexProp.Length == 0) throw new Exception($"could not find and indexer property on type { expr.Type } that accepts { index.Type } as argument");
            if (indexProp.Length > 1) throw new Exception($"more than one indexer found on type { expr.Type } that accepts { index.Type } as argument");
            return indexProp[0];
        }

        public static IEnumerable<CSharpArgumentInfo> GetBinderArguments(int count)
        {
            var arr = new CSharpArgumentInfo[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null);
            }
            return arr;
        }

        public static Expression? ApplyBinder(DynamicMetaObjectBinder binder, bool throwException, params Expression[] expressions)
        {
            // This null just works and C# compiler seems to produce too, I think they have bug in the type annotations
            //                                                vvvv
            var result = binder.Bind(DynamicMetaObject.Create(null!, expressions[0]),
                expressions.Skip(1).Select(e =>
                    DynamicMetaObject.Create(null!, e)).ToArray()
            );

            if (result.Expression.NodeType == ExpressionType.Convert)
            {
                var convert = (UnaryExpression)result.Expression;
                return convert.Operand;
            }
            if (result.Expression.NodeType == ExpressionType.Throw)
            {
                if (throwException)
                {
                    // throw the exception
                    Expression.Lambda(result.Expression).Compile(preferInterpretation: true).DynamicInvoke();
                }
                else return null;
            }
            return result.Expression;
        }


        /// <summary> Unwraps pointless operations with the expression, such as casts, boxing, ToBrowserLocalTime, ToString, ... Used to extract the actual property backing this value, used for validation expressions, ... </summary>
        public static Expression UnwrapPassthroughOperations(Expression expr)
        {
            while (true)
            {
                // unwrap type conversions, negations, ...
                if (expr is UnaryExpression unary)
                    expr = unary.Operand;
                // unwrap some method invocations
                else if (expr is MethodCallExpression boxCall && boxCall.Method.DeclaringType == typeof(BoxingUtils))
                    expr = boxCall.Arguments.First();
                else if (expr is MethodCallExpression { Method.Name: nameof(DateTimeExtensions.ToBrowserLocalTime) } dtMethodCall && dtMethodCall.Method.DeclaringType == typeof(DateTimeExtensions))
                    expr = dtMethodCall.Object ?? dtMethodCall.Arguments.First();
                else if (expr is MethodCallExpression { Method.Name: nameof(object.ToString) } toStringMethodCall)
                    expr = toStringMethodCall.Object ?? toStringMethodCall.Arguments.First();
                else if (expr is MethodCallExpression { Method.Name: nameof(Enums.ToEnumString) } toEnumStringMethodCall && toEnumStringMethodCall.Method.DeclaringType == typeof(Enums))
                    expr = toEnumStringMethodCall.Object ?? toEnumStringMethodCall.Arguments.First();
                // unwrap binary operation with a constant
                else if (expr is BinaryExpression { Right.NodeType: ExpressionType.Constant } binaryLeft)
                    expr = binaryLeft.Left;
                else if (expr is BinaryExpression { Left.NodeType: ExpressionType.Constant } binaryRight)
                    expr = binaryRight.Right;
                else
                    return expr;
            }
        }
    }
}
