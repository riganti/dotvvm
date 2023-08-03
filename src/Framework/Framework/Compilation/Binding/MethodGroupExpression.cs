using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

namespace DotVVM.Framework.Compilation.Binding
{

    public sealed class MethodGroupExpression : Expression
    {
        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => typeof(Delegate);

        public override bool CanReduce => GetMethod() != null;

        public Expression Target { get; }
        public string MethodName { get; }
        public Type[]? TypeArgs { get; }
        public List<MethodInfo>? Candidates { get; set; }
        public bool HasExtensionCandidates { get; }
        public bool IsStatic => Target is StaticClassIdentifierExpression;

        private static MethodInfo CreateDelegateMethodInfo = typeof(Delegate).GetMethod("CreateDelegate", new[] { typeof(Type), typeof(object), typeof(MethodInfo) })!;

        public MethodGroupExpression(Expression target, string methodName, Type[]? typeArgs = null, List<MethodInfo>? candidates = null, bool hasExtensionCandidates = false)
        {
            Target = target;
            MethodName = methodName;
            TypeArgs = typeArgs;
            Candidates = candidates;
            HasExtensionCandidates = hasExtensionCandidates;
        }

        public Expression? CreateDelegateExpression(Type delegateType, bool throwException = true)
        {
            if (delegateType == null || delegateType == typeof(object) || delegateType == typeof(Delegate)) return CreateDelegateExpression();
            if (!delegateType.IsDelegate(out var invokeMethod))
                if (throwException) throw new Exception("Could not convert method group expression to a non delegate type."); else return null;
            var parameters = invokeMethod.GetParameters().Select(p => p.ParameterType).ToArray();
            var method = Target.Type.GetMethods(BindingFlags.Public | (IsStatic ? BindingFlags.Static : BindingFlags.Instance))
                .FirstOrDefault(m => m.Name == MethodName && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameters) && m.ReturnType == invokeMethod.ReturnType);
            if (method == null)
                if (throwException) throw new Exception($"Could not convert method group '{Target.Type.Name}.{ MethodName }' to delegate '{ delegateType.FullName }'");
                else return null;

            // create lambda expression
            var args = method.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            var call = IsStatic ? Expression.Call(method, args) : Expression.Call(Target, method, args);
            return Expression.Lambda(delegateType, call, args);
        }

        private MethodInfo? GetMethod()
            => Target.Type.GetMethod(MethodName, BindingFlags.Public | (IsStatic ? BindingFlags.Static : BindingFlags.Instance));

        private Exception Error()
        {
            if (Target.Type == typeof(UnknownTypeSentinel))
                return new Exception($"Type of '{Target}' could not be resolved.");

            var candidateMethods =
                Target.Type
                .GetAllMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.Name == MethodName)
                .ToArray();

            if (!candidateMethods.Any())
                return new Exception($"Method '{Target.Type.ToCode(stripNamespace: true)}.{MethodName}' not found.");
            if (!candidateMethods.Any(m => m.IsStatic == this.IsStatic))
                return new Exception($"{(this.IsStatic ? "Static" : "Instance")} method '{Target.Type.ToCode(stripNamespace: true)}.{MethodName}' not found, but {(this.IsStatic ? "an instance" : "a static")} method exists.");
            var matchingMethods = candidateMethods.Where(m => m.IsStatic == this.IsStatic).ToArray();
            if (!matchingMethods.Any())
                return new Exception($"Method '{Target.Type.ToCode(stripNamespace: true)}.{MethodName}' not found, but a private method exists.");
            if (matchingMethods.Length > 1)
                return new Exception($"Multiple matching overloads of method '{Target.Type.ToCode(stripNamespace: true)}.{MethodName}' exist.");
            throw new Exception("Internal error");
        }

        public Expression CreateDelegateExpression()
        {
            var methodInfo = GetMethod();
            if (methodInfo == null) throw new Exception($"cannot create delegate from method '{ MethodName }' on type '{ Target.Type.FullName }'");

            var args = methodInfo.GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();

            var call = IsStatic ? Expression.Call(methodInfo, args) : Expression.Call(Target, methodInfo, args);
            return Expression.Lambda(call, args);
        }
        public Expression CreateMethodCall(IEnumerable<Expression> args, MemberExpressionFactory memberExpressionFactory)
        {
            var argsArray = args.ToArray();
            if (Array.FindIndex(argsArray, a => a is null || a.Type == typeof(UnknownTypeSentinel)) is var argIdx && argIdx >= 0)
                throw new Exception($"Argument {argIdx} is invalid: {this.MethodName}({string.Join(", ", argsArray.Select(a => a))})");

            if (IsStatic)
            {
                return memberExpressionFactory.CallMethod(((StaticClassIdentifierExpression)Target).Type, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, MethodName, TypeArgs, argsArray);
            }
            else
            {
                return memberExpressionFactory.CallMethod(Target, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy, MethodName, TypeArgs, argsArray);
            }
        }

        public override Expression Reduce()
        {
            return CreateDelegateExpression();
        }
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (GetMethod() is null) throw Error();

            return base.VisitChildren(visitor);
        }
        protected override Expression Accept(ExpressionVisitor visitor)
        {
            if (GetMethod() is null) throw Error();

            return base.Accept(visitor);
        }

        public override string ToString()
        {
            return $"{Target}.{MethodName}";
        }
    }
}
