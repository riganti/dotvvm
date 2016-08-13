using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptTranslator
    {
        public static string CompileToJavascript(Expression binding, DataContextStack dataContext)
        {
            var translator = new JavascriptTranslator();
            translator.DataContexts = dataContext;
            var script = translator.Translate(binding).Trim();
            if (binding.NodeType == ExpressionType.MemberAccess && script.EndsWith("()", StringComparison.Ordinal)) script = script.Remove(script.Length - 2);
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
            if (method.DeclaringType.IsInterface)
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
            var lengthMethod = new StringJsMethodCompiler("{0}.length");
            AddPropertyGetterTranslator(typeof(Array), nameof(Array.Length), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(ICollection<>), nameof(ICollection.Count), lengthMethod);
            AddPropertyGetterTranslator(typeof(string), nameof(string.Length), lengthMethod);
            AddMethodTranslator(typeof(object), "ToString", new StringJsMethodCompiler("String({0})", (m, c, a) => ToStringCheck(c)), 0);
            AddMethodTranslator(typeof(Convert), "ToString", new StringJsMethodCompiler("String({1})", (m, c, a) => ToStringCheck(a[0])), 1, true);
            //AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), lengthMethod, new[] { typeof(IEnumerable) });

            BindingPageInfo.RegisterJavascriptTranslations();
        }

        static bool ToStringCheck(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
            return expr.Type.IsPrimitive;
        }

        public DataContextStack DataContexts { get; set; }

        public string ParenthesizedTranslate(Expression parent, Expression expression)
        {
            if (NeedsParens(parent, expression))
            {
                return "(" + Translate(expression) + ")";
            }
            else
            {
                return Translate(expression);
            }
        }

        public string Translate(Expression expression)
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

        public string TranslateAssing(BinaryExpression expression)
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

        private string SetProperty(string target, PropertyInfo property, string value)
        {
            if (ViewModelJsonConverter.IsPrimitiveType(property.PropertyType))
            {
                return target + "." + property.Name + "(" + value + ")";
            }
            else
            {
                return $"dotvvm.serialization.deserialize({ value }, { target }.{ property.Name })";
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

        public string TranslateConditional(ConditionalExpression expression)
        {
            return $"{ ParenthesizedTranslate(expression, expression.Test) } ? { ParenthesizedTranslate(expression, expression.IfTrue) } : { ParenthesizedTranslate(expression, expression.IfFalse) }";
        }

        public string TranslateIndex(IndexExpression expression, bool setter = false)
        {
            var target = Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var method = setter ? expression.Indexer.SetMethod : expression.Indexer.GetMethod;

            var result = TryTranslateMethodCall(target, args, method, expression.Object, expression.Arguments.ToArray());
            if (result != null) return result;
            return target + "[" + args.Single() + "]()";
        }

        public string TranslateParameter(ParameterExpression expression)
        {
            if (expression.Name == "_this") return "$data";
            if (expression.Name == "_parent") return "$parent";
            if (expression.Name == "_root") return "$root";
            if (expression.Name.StartsWith("_parent", StringComparison.Ordinal)) return $"$parents[{ int.Parse(expression.Name.Substring("_parent".Length)) - 1 }]";
            if (expression.Name == "_control")
            {
                var c = DataContexts.Parents().Count();
                string context = string.Concat(Enumerable.Repeat("$parentContext.", c));
                return context + "$control";
            }
            if (WriteUnknownParameters && !string.IsNullOrEmpty(expression.Name)) return expression.Name;
            else throw new NotSupportedException();
        }

        public string TranslateConstant(ConstantExpression expression)
        {
            return JavascriptCompilationHelper.CompileConstant(expression.Value);
        }

        public string TranslateMethodCall(MethodCallExpression expression)
        {
            var thisExpression = expression.Object == null ? null : ParenthesizedTranslate(expression, expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var result = TryTranslateMethodCall(thisExpression, args, expression.Method, expression.Object, expression.Arguments.ToArray());
            if (result == null)
                throw new NotSupportedException($"Method { expression.Method.DeclaringType.Name }.{ expression.Method.Name } can not be translated to Javascript");
            return result;
        }

        protected string TryTranslateMethodCall(string context, string[] args, MethodInfo method, Expression contextExpression, Expression[] argsExpressions)
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
                    var map = method.DeclaringType.GetInterfaceMap(iface);
                    var imIndex = Array.IndexOf(map.TargetMethods, method);
                    if (imIndex >= 0 && MethodTranslators.ContainsKey(map.InterfaceMethods[imIndex]) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
                        return MethodTranslators[map.InterfaceMethods[imIndex]].TranslateCall(context, args, method);
                }
            }
            if (method.DeclaringType.IsGenericType && !method.DeclaringType.IsGenericTypeDefinition)
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
        public string TranslateBinary(BinaryExpression expression)
        {
            var left = ParenthesizedTranslate(expression, expression.Left);
            var right = ParenthesizedTranslate(expression, expression.Right);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { left, right }, expression.Method, null, new[] { expression.Left, expression.Right });
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
                case ExpressionType.ArrayIndex: op = "{0}[{1}]"; break;
                default:
                    throw new NotSupportedException($"Unary operator of type { expression.NodeType } is not supported");
            }
            if (!op.Contains('{')) op = "{0}" + op + "{1}";
            return string.Format(op, left, right);
        }

        public string TranslateUnary(UnaryExpression expression)
        {
            var operand = ParenthesizedTranslate(expression, expression.Operand);
            var method = expression.Method;
            if (method != null)
            {
                var mTranslate = TryTranslateMethodCall(null, new[] { operand }, expression.Method, null, new[] { expression.Operand });
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
            var getter = (expression.Member as PropertyInfo)?.GetMethod;
            if (expression.Expression == null)
            {
                // static
                return TryTranslateMethodCall(null, new string[0], getter, null, new Expression[0]) ??
                    JavascriptCompilationHelper.CompileConstant((
                        ((expression.Member as FieldInfo)?.GetValue(null) ?? (expression.Member as PropertyInfo)?.GetValue(null))));
            }
            else
            {
                return TryTranslateMethodCall(ParenthesizedTranslate(expression, expression.Expression), new string[0], getter, expression.Expression, new Expression[0]) ??
                    TranslateViewModelProperty(ParenthesizedTranslate(expression, expression.Expression), expression.Member);
            }
        }

        public string TranslateViewModelProperty(string context, MemberInfo propInfo)
        {
            if (propInfo is FieldInfo) throw new NotSupportedException($"Field '{propInfo.Name}' cannot be translated to knockout binding. Use property with public getter and setter.");
            var protection = propInfo.GetCustomAttribute<ProtectAttribute>();
            if (protection != null && protection.Settings == ProtectMode.EncryptData)
                throw new NotSupportedException($"Encrypted property '{propInfo.Name}' cannot be used in binding.");
            // Bind(None) can make sense to translate, since it can be used as client-only property
            return context + "." + propInfo.Name + "()";
        }
    }
}