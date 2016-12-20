using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptTranslator
    {
        public static JsNode CompileToJavascript(Expression binding, DataContextStack dataContext)
        {
            var translator = new JavascriptTranslator();
            translator.DataContexts = dataContext;
            var script = translator.Translate(binding);
            //if (binding.NodeType == ExpressionType.MemberAccess && script.EndsWith("()", StringComparison.Ordinal)) script = script.Remove(script.Length - 2);
            return script;
        }

        public static readonly Dictionary<MethodInfo, IJsMethodTranslator> MethodTranslators = new Dictionary<MethodInfo, IJsMethodTranslator>();
        public static readonly HashSet<Type> Interfaces = new HashSet<Type>();

        public bool WriteUnknownParameters { get; set; } = true;

        static JavascriptTranslator()
        {
            AddDefaultMethodTranslators();
        }

        public static void AddMethodTranslator(Type declaringType, string methodName, IJsMethodTranslator translator, Type[] parameters = null)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName);
            if (parameters != null)
            {
                methods = methods.Where(m =>
                {
                    var mp = m.GetParameters();
                    return mp.Length == parameters.Length && parameters.Zip(mp, (specified, method) => method.ParameterType.IsAssignableFrom(specified)).All(t => t);
                });
            }
            AddMethodTranslator(methods.Single(), translator);
        }

        public static void AddMethodTranslator(Type declaringType, string methodName, IJsMethodTranslator translator, int parameterCount, bool allowMultipleMethods = false)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName)
                .Where(m => m.GetParameters().Length == parameterCount)
                .ToArray();
            if (methods.Length > 1 && !allowMultipleMethods) throw new Exception("more then one methods");
            foreach (var method in methods)
            {
                AddMethodTranslator(method, translator);
            }
        }

        public static void AddMethodTranslator(MethodInfo method, IJsMethodTranslator translator)
        {
            MethodTranslators.Add(method, translator);
            if (method.DeclaringType.GetTypeInfo().IsInterface)
                Interfaces.Add(method.DeclaringType);
        }

        public static void AddPropertySetterTranslator(Type declaringType, string methodName, IJsMethodTranslator translator)
        {
            var property = declaringType.GetProperty(methodName);
            AddMethodTranslator(property.SetMethod, translator);
        }

        public static void AddPropertyGetterTranslator(Type declaringType, string methodName, IJsMethodTranslator translator)
        {
            var property = declaringType.GetProperty(methodName);
            AddMethodTranslator(property.GetMethod, translator);
        }

        public static void AddDefaultMethodTranslators()
        {
            var lengthMethod = new GenericMethodCompiler(a => a[0].Member("length"));
            AddPropertyGetterTranslator(typeof(Array), nameof(Array.Length), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection<>), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(string), nameof(string.Length), lengthMethod);
            AddMethodTranslator(typeof(object), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[0]), (m, c, a) => ToStringCheck(c)), 0);
            AddMethodTranslator(typeof(Convert), "ToString", new GenericMethodCompiler(
                a => new JsIdentifierExpression("String").Invoke(a[1]), (m, c, a) => ToStringCheck(a[0])), 1, true);
            //AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), lengthMethod, new[] { typeof(IEnumerable) });

            BindingPageInfo.RegisterJavascriptTranslations();
        }

        static bool ToStringCheck(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
            return expr.Type.GetTypeInfo().IsPrimitive;
        }

        public DataContextStack DataContexts { get; set; }

        public JsExpression ParenthesizedTranslate(Expression parent, Expression expression)
        {
            if (NeedsParens(parent, expression))
            {
                return new JsParenthesizedExpression(Translate(expression));
            }
            else
            {
                return Translate(expression);
            }
        }

        public JsExpression Translate(Expression expression)
        {
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

                case ExpressionType.Index:
                    return TranslateIndex((IndexExpression)expression);

                case ExpressionType.Assign:
                    return TranslateAssing((BinaryExpression)expression);
            }
            if (expression is BinaryExpression) return TranslateBinary((BinaryExpression)expression);
            else if (expression is UnaryExpression) return TranslateUnary((UnaryExpression)expression);

            throw new NotSupportedException($"expression type { expression.NodeType } can not be transaled to Javascript");
        }

        public JsExpression TranslateAssing(BinaryExpression expression)
        {
            var property = expression.Left as MemberExpression;
            if (property != null)
            {
                var target = Translate(property.Expression);
                var value = Translate(expression.Right);
                return TryTranslateMethodCall(target, new[] { value }, (property.Member as PropertyInfo)?.SetMethod, property.Expression, new[] { expression.Right }) ??
                    SetProperty(target, property.Member as PropertyInfo, value);
            }
            throw new NotSupportedException($"can not assign expression of type {expression.Left.NodeType}");
        }

        private JsExpression SetProperty(JsExpression target, PropertyInfo property, JsExpression value)
        {
            if (ViewModelJsonConverter.IsPrimitiveType(property.PropertyType))
            {
                return target.Member(property.Name).Invoke(value);
            }
            else
            {
                return new JsIdentifierExpression("dotvvm").Member("serialization").Member("deserialize").Invoke(value, target.Member(property.Name));
            }
        }

        /// <summary>
        /// Determines if the expression will have to be parenthised when called from parent expression
        /// </summary>
        public bool NeedsParens(Expression parent, Expression expression)
        {
            var exType = expression.NodeType;
            switch (exType)
            {
                case ExpressionType.ArrayLength:
                case ExpressionType.ArrayIndex:
                case ExpressionType.Call:
                case ExpressionType.Constant:
                case ExpressionType.Invoke:
                case ExpressionType.ListInit:
                case ExpressionType.MemberAccess:
                case ExpressionType.MemberInit:
                case ExpressionType.New:
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                case ExpressionType.Parameter:
                case ExpressionType.Default:
                case ExpressionType.Index:
                    return false;
            }
            // TODO: more clever brackets
            return true;
        }

        public JsExpression TranslateConditional(ConditionalExpression expression)
        {
            return new JsConditionalExpression(
                ParenthesizedTranslate(expression, expression.Test),
                ParenthesizedTranslate(expression, expression.IfTrue),
                ParenthesizedTranslate(expression, expression.IfFalse));
        }

        public JsExpression TranslateIndex(IndexExpression expression, bool setter = false)
        {
            var target = Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var method = setter ? expression.Indexer.SetMethod : expression.Indexer.GetMethod;

            var result = TryTranslateMethodCall(target, args, method, expression.Object, expression.Arguments.ToArray());
            if (result != null) return result;
            return new JsIndexerExpression(target, args.Single());
        }

        public JsExpression TranslateParameter(ParameterExpression expression)
        {
            if (expression.Name == "_this") return new JsIdentifierExpression("$data");
            if (expression.Name == "_parent") return new JsIdentifierExpression("$parent");
            if (expression.Name == "_root") return new JsIdentifierExpression("$root");
            if (expression.Name.StartsWith("_parent", StringComparison.Ordinal))
			{
				var pIndex = int.Parse(expression.Name.Substring("_parent".Length));
				if (pIndex == 0) return new JsIdentifierExpression("$data");
				else if (pIndex == 1) return new JsIdentifierExpression("$parent");
				else return new JsIndexerExpression(new JsIdentifierExpression("$parents"), new JsLiteral(pIndex - 1));
			}

			if (expression.Name == "_control")
            {
                var c = DataContexts.Parents().Count();
                JsExpression context = null;
                for (int i = 0; i < c; i++) {
                    context = context.Member("$parentContext");
                }
                return context.Member("$control");
            }
            if (WriteUnknownParameters && !string.IsNullOrEmpty(expression.Name)) return new JsIdentifierExpression(expression.Name);
            else throw new NotSupportedException();
        }

        public JsLiteral TranslateConstant(ConstantExpression expression)
        {
            return new JsLiteral(expression.Value);
        }

        public JsExpression TranslateMethodCall(MethodCallExpression expression)
        {
            var thisExpression = expression.Object == null ? null : ParenthesizedTranslate(expression, expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var result = TryTranslateMethodCall(thisExpression, args, expression.Method, expression.Object, expression.Arguments.ToArray());
            if (result == null)
                throw new NotSupportedException($"Method { expression.Method.DeclaringType.Name }.{ expression.Method.Name } can not be translated to Javascript");
            return result;
        }

        protected JsExpression TryTranslateMethodCall(JsExpression context, JsExpression[] args, MethodInfo method, Expression contextExpression, Expression[] argsExpressions)
        {
            if (method == null) return null;
            IJsMethodTranslator translator;
            if (MethodTranslators.TryGetValue(method, out translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
            {
                return translator.TranslateCall(context, args, method);
            }
            if (method.IsGenericMethod)
            {
                var genericMethod = method.GetGenericMethodDefinition();
                if (MethodTranslators.TryGetValue(genericMethod, out translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
                {
                    return translator.TranslateCall(context, args, method);
                }
            }

            foreach (var iface in method.DeclaringType.GetInterfaces())
            {
                if (Interfaces.Contains(iface))
                {
                    var map = method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(iface);
                    var imIndex = Array.IndexOf(map.TargetMethods, method);
                    if (imIndex >= 0 && MethodTranslators.TryGetValue(map.InterfaceMethods[imIndex], out translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
                        return translator.TranslateCall(context, args, method);
                }
            }
            if (method.DeclaringType.GetTypeInfo().IsGenericType && !method.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericType = method.DeclaringType.GetGenericTypeDefinition();
                var m2 = genericType.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m2 != null)
                {
                    var r2 = TryTranslateMethodCall(context, args, m2, contextExpression, argsExpressions);
                    if (r2 != null) return r2;
                }
            }
            if (method.DeclaringType == typeof(Array))
            {
                var m2 = typeof(Array).GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m2 != null)
                {
                    var r2 = TryTranslateMethodCall(context, args, m2, contextExpression, argsExpressions);
                    if (r2 != null) return r2;
                }
            }
            var baseMethod = method.GetBaseDefinition();
            if (baseMethod != null && baseMethod != method) return TryTranslateMethodCall(context, args, baseMethod, contextExpression, argsExpressions);
            else return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public JsExpression TranslateBinary(BinaryExpression expression)
        {
            var left = ParenthesizedTranslate(expression, expression.Left);
            var right = ParenthesizedTranslate(expression, expression.Right);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { left, right }, expression.Method, null, new[] { expression.Left, expression.Right });
                if (mTranslate != null) return mTranslate;
            }
            BinaryOperatorType op;
            switch (expression.NodeType)
            {
                case ExpressionType.Equal: op = BinaryOperatorType.Equal; break;
                case ExpressionType.NotEqual: op = BinaryOperatorType.NotEqual; break;
                case ExpressionType.AndAlso: op = BinaryOperatorType.ConditionalAnd; break;
                case ExpressionType.OrElse: op = BinaryOperatorType.ConditionalOr; break;
                case ExpressionType.GreaterThan: op = BinaryOperatorType.Greater; break;
                case ExpressionType.LessThan: op = BinaryOperatorType.Less; break;
                case ExpressionType.GreaterThanOrEqual: op = BinaryOperatorType.GreaterOrEqual; break;
                case ExpressionType.LessThanOrEqual: op = BinaryOperatorType.Less; break;
                case ExpressionType.AddChecked:
                case ExpressionType.Add: op = BinaryOperatorType.Plus; break;
                case ExpressionType.SubtractChecked:
                case ExpressionType.Subtract: op = BinaryOperatorType.Minus; break;
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.Divide: op = BinaryOperatorType.Divide; break;
                case ExpressionType.Modulo: op = BinaryOperatorType.Modulo; break;
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Multiply: op = BinaryOperatorType.Times; break;
                case ExpressionType.LeftShift: op = BinaryOperatorType.LeftShift; break;
                case ExpressionType.RightShift: op = BinaryOperatorType.UnsignedRightShift; break;
                case ExpressionType.And: op = BinaryOperatorType.BitwiseAnd; break;
                case ExpressionType.Or: op = BinaryOperatorType.BitwiseOr; break;
                case ExpressionType.ExclusiveOr: op = BinaryOperatorType.BitwiseXOr; break;
                case ExpressionType.Coalesce: op = BinaryOperatorType.ConditionalOr; break;
                case ExpressionType.ArrayIndex: return new JsIndexerExpression(left, right);
                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            return new JsBinaryExpression(left, op, right);
        }

        public JsExpression TranslateUnary(UnaryExpression expression)
        {
            var operand = ParenthesizedTranslate(expression, expression.Operand);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { operand }, expression.Method, null, new[] { expression.Operand });
                if (mTranslate != null) return mTranslate;
            }
            UnaryOperatorType op;
            switch (expression.NodeType)
            {
                case ExpressionType.NegateChecked:
                case ExpressionType.Negate:
                    op = UnaryOperatorType.Minus;
                    break;

                case ExpressionType.UnaryPlus:
                    op = UnaryOperatorType.Plus;
                    break;

                case ExpressionType.Not:
                    if (expression.Operand.Type == typeof(bool))
                        op = UnaryOperatorType.LogicalNot;
                    else op = UnaryOperatorType.BitwiseNot;
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
            return new JsUnaryExpression(op, operand);
        }

        public JsExpression TranslateMemberAccess(MemberExpression expression)
        {
            var getter = (expression.Member as PropertyInfo)?.GetMethod;
            if (expression.Expression == null)
            {
                // static
                return TryTranslateMethodCall(null, new JsExpression[0], getter, null, new Expression[0]) ??
                    new JsLiteral((
                        ((expression.Member as FieldInfo)?.GetValue(null) ?? (expression.Member as PropertyInfo)?.GetValue(null))));
            }
            else
            {
                return TryTranslateMethodCall(ParenthesizedTranslate(expression, expression.Expression), new JsExpression[0], getter, expression.Expression, new Expression[0]) ??
                    TranslateViewModelProperty(ParenthesizedTranslate(expression, expression.Expression), expression.Member);
            }
        }

        public JsExpression TranslateViewModelProperty(JsExpression context, MemberInfo propInfo)
        {
            if (propInfo is FieldInfo) throw new NotSupportedException($"Field '{propInfo.Name}' cannot be translated to knockout binding. Use property with public getter and setter.");
            var protection = propInfo.GetCustomAttribute<ProtectAttribute>();
            if (protection != null && protection.Settings == ProtectMode.EncryptData)
                throw new NotSupportedException($"Encrypted property '{propInfo.Name}' cannot be used in binding.");
            // Bind(None) can make sense to translate, since it can be used as client-only property
            return new JsInvocationExpression(new JsMemberAccessExpression(context, propInfo.Name));
        }
    }
}