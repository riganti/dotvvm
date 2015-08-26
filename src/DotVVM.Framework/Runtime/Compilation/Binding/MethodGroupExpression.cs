using DotVVM.Framework.Parser.Binding.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{

    public class MethodGroupExpression : Expression
    {
        public Expression Target { get; set; }
        public string MethodName { get; set; }
        public Type[] TypeArgs { get; set; }

        public bool IsStatic => Target is ConstantExpression && ((ConstantExpression)Target).Value == null;

        private static MethodInfo CreateDelegateFromStringMethodInfo = typeof(Delegate).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(string) });
        private static MethodInfo CreateDelegateMethodInfo = typeof(Delegate).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(MethodInfo) });
        public Expression CreateDelegateExpression(Type delegateType)
        {
            if (delegateType == null || delegateType == typeof(object)) return CreateDelegateExpression();
            if (IsStatic)
                return Expression.Constant(Delegate.CreateDelegate(delegateType, Target.Type, MethodName));
            else
                return Expression.Call(CreateDelegateFromStringMethodInfo, Expression.Constant(delegateType), Target, Expression.Constant(MethodName));
        }

        private static Type GetDelegateType(Type returnType, Type[] args)
        {
            if (returnType == null || returnType == typeof(void))
            {
                return Type.GetType("System.Action`" + args.Length).MakeGenericType(args);
            }
            else
            {
                return Type.GetType("System.Func`" + (args.Length + 1)).MakeGenericType(args.Concat(new[] { returnType }).ToArray());
            }
        }

        private static Type GetDelegateType(MethodInfo methodInfo)
        {
            return GetDelegateType(methodInfo.ReturnType, methodInfo.GetParameters().Select(a => a.ParameterType).ToArray());
        }

        public Expression CreateDelegateExpression()
        {
            var methodInfo = Target.Type.GetMethod(MethodName);
            if (methodInfo == null) throw new Exception($"can't create delegate from method { MethodName } on type { Target.Type.FullName }");

            if (IsStatic)
                return Expression.Constant(Delegate.CreateDelegate(GetDelegateType(methodInfo), methodInfo));
            else
                return Expression.Call(CreateDelegateMethodInfo, Expression.Constant(GetDelegateType(methodInfo)), Target, Expression.Constant(methodInfo));
        }
        public Expression CreateMethodCall(IEnumerable<Expression> args)
        {
            return Expression.Call(Target, MethodName, TypeArgs, args.ToArray());
        }
    }
}
