using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Compilation.Javascript
{
    public class JavascriptTranslator
    {
        public static object KnockoutContextParameter = new object();
        public static object KnockoutViewModelParameter = new object();
        public static object CurrentIndexParameter = new object();

        public static JsExpression CompileToJavascript(Expression binding, DataContextStack dataContext, IViewModelSerializationMapper mapper)
        {
            var translator = new JavascriptTranslator(dataContext);
            var script = translator.Translate(binding);
            //if (binding.NodeType == ExpressionType.MemberAccess && script.EndsWith("()", StringComparison.Ordinal)) script = script.Remove(script.Length - 2);
            script.AcceptVisitor(new JsViewModelPropertyAdjuster(mapper));
            return script;
        }

        public static JsExpression RemoveTopObservables(JsExpression expression)
        {
            foreach (var leaf in expression.GetLeafResultNodes())
            {
                JsExpression replacement = null;
                if (leaf is JsInvocationExpression invocation && invocation.Annotation<ObservableUnwrapInvocationAnnotation>() != null)
                {
                    replacement = invocation.Target;
                }
                else if (leaf is JsMemberAccessExpression member && member.MemberName == "$data" && member.Target is JsSymbolicParameter par && par.Symbol == JavascriptTranslator.KnockoutContextParameter ||
                    leaf is JsSymbolicParameter param && param.Symbol == JavascriptTranslator.KnockoutViewModelParameter)
                {
                    replacement = new JsSymbolicParameter(KnockoutContextParameter).Member("$rawData")
                        .WithAnnotation(leaf.Annotation<ViewModelInfoAnnotation>());
                }

                if (replacement != null)
                {
                    if (leaf.Parent == null) expression = replacement;
                    else leaf.ReplaceWith(replacement);
                }
            }
            return expression;
        }

        public static (JsExpression context, JsExpression data) GetKnockoutContextParameters(int dataContextLevel)
        {
            JsExpression currentContext = new JsSymbolicParameter(KnockoutContextParameter);
            for (int i = 0; i < dataContextLevel; i++) currentContext = currentContext.Member("$parentContext");

            var currentData = dataContextLevel == 0 ? new JsSymbolicParameter(KnockoutContextParameter).Member("$data") :
                              dataContextLevel == 1 ? new JsSymbolicParameter(KnockoutContextParameter).Member("$parent") :
                              new JsSymbolicParameter(KnockoutContextParameter).Member("$parents").Indexer(new JsLiteral(dataContextLevel - 1));
            return (currentContext, currentData);
        }
        public static ParametrizedCode AdjustKnockoutScriptContext(ParametrizedCode expression, int dataContextLevel)
        {
            if (dataContextLevel == 0) return expression;
            var (contextExpresion, dataExpression) = GetKnockoutContextParameters(dataContextLevel);
            var (context, data) = (CodeParameterAssignment.FromExpression(contextExpresion), CodeParameterAssignment.FromExpression(dataExpression));
            return expression.AssignParameters(o =>
                o == KnockoutContextParameter ? context :
                o == KnockoutViewModelParameter ? data :
                o == CurrentIndexParameter ? CodeParameterAssignment.FromExpression(contextExpresion.Member("$index").Invoke()) :
                default(CodeParameterAssignment)
            );
        }

        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(JsExpression expression, bool allowDataGlobal = true, int dataContextLevel = 0) =>
            FormatKnockoutScript(expression.FormatParametrizedScript(), allowDataGlobal, dataContextLevel);
        /// <summary>
        /// Get's Javascript code that can be executed inside knockout data-bind attribute.
        /// </summary>
        public static string FormatKnockoutScript(ParametrizedCode expression, bool allowDataGlobal = true, int dataContextLevel = 0)
        {
            // TODO(exyi): more symbolic parameters
            return AdjustKnockoutScriptContext(expression, dataContextLevel)
                .ToString(o => o == KnockoutContextParameter ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("$context"), true) :
                               o == KnockoutViewModelParameter ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("$data"), allowDataGlobal) :
                               o == CurrentIndexParameter ? CodeParameterAssignment.FromExpression(new JsIdentifierExpression("$index").Invoke()) :
                               throw new Exception());
        }

        public static readonly Dictionary<MethodInfo, IJsMethodTranslator> MethodTranslators = new Dictionary<MethodInfo, IJsMethodTranslator>();
        public static readonly HashSet<Type> Interfaces = new HashSet<Type>();
        private readonly Dictionary<DataContextStack, int> ContextMap;

        public bool WriteUnknownParameters { get; set; } = true;

        static JavascriptTranslator()
        {
            AddDefaultMethodTranslators();
        }

        public static void AddMethodTranslator(Type declaringType, string methodName, IJsMethodTranslator translator, Type[] parameters = null, bool allowGeneric = true)
        {
            var methods = declaringType.GetMethods()
                .Where(m => m.Name == methodName && (allowGeneric || !m.IsGenericMethod));
            if (parameters != null)
            {
                methods = methods.Where(m => {
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

            JsExpression indexer(JsExpression[] args, MethodInfo method) =>
                BuildIndexer(args[0], args[1], method.DeclaringType.GetProperty("Item"));
            AddMethodTranslator(typeof(IList), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(IList<>), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(List<>), "get_Item", new GenericMethodCompiler(indexer));
            AddMethodTranslator(typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Static | BindingFlags.Public), new GenericMethodCompiler((args, method) =>
                BuildIndexer(args[1], args[2], method)));
            AddPropertyGetterTranslator(typeof(Nullable<>), "Value", new GenericMethodCompiler((args, method) => args[0]));
            //AddMethodTranslator(typeof(Enumerable), nameof(Enumerable.Count), lengthMethod, new[] { typeof(IEnumerable) });

            JavascriptTranslator.AddMethodTranslator(typeof(Api), nameof(Api.RefreshOnChange),
                new GenericMethodCompiler(a => a[2] is JsIdentifierExpression || a[2] is JsMemberAccessExpression member && member.Target is JsSymbolicParameter && !member.Target.HasAnnotation<ResultIsObservableAnnotation>() ?
                    new JsIdentifierExpression("dotvvm").Member("apiRefreshOn").Invoke(
                        a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                        a[2].WithAnnotation(ShouldBeObservableAnnotation.Instance)) :
                    new JsIdentifierExpression("dotvvm").Member("apiRefreshOn").Invoke(
                        a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                        new JsIdentifierExpression("ko").Member("pureComputed").Invoke(new JsFunctionExpression(
                            parameters: Enumerable.Empty<JsIdentifier>(),
                            bodyBlock: new JsBlockStatement(new JsReturnStatement(a[2])))))
                ));
            JavascriptTranslator.AddMethodTranslator(typeof(Api), nameof(Api.RefreshOnEvent),
                new GenericMethodCompiler(a =>
                    new JsIdentifierExpression("dotvvm").Member("apiRefreshOn").Invoke(
                        a[1].WithAnnotation(ShouldBeObservableAnnotation.Instance),
                        new JsIdentifierExpression("dotvvm").Member("eventHub").Member("get").Invoke(a[2]))));
            BindingPageInfo.RegisterJavascriptTranslations();
        }

        static bool ToStringCheck(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Convert) expr = ((UnaryExpression)expr).Operand;
            return expr.Type.GetTypeInfo().IsPrimitive;
        }

        public DataContextStack DataContext { get; }

        public JavascriptTranslator(DataContextStack dataContext)
        {
            this.ContextMap = dataContext.EnumerableItems().Select((a, i) => (a, i)).ToDictionary(a => a.Item1, a => a.Item2);
            this.DataContext = dataContext;
        }

        public JsExpression Translate(Expression expression)
        {
            if (expression.GetParameterAnnotation() is BindingParameterAnnotation annotation)
                return TranslateParameter(expression, annotation);

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
            if (expression is BinaryExpression)
            {
                return TranslateBinary((BinaryExpression)expression);
            }
            else if (expression is UnaryExpression)
            {
                return TranslateUnary((UnaryExpression)expression);
            }

            throw new NotSupportedException($"The expression type {expression.NodeType} can not be translated to Javascript!");
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
            throw new NotSupportedException($"Can not assign expression of type {expression.Left.NodeType}!");
        }

        private JsExpression SetProperty(JsExpression target, PropertyInfo property, JsExpression value)
        {
            return new JsAssignmentExpression(this.TranslateViewModelProperty(target, property), value);
        }

        public JsExpression TranslateConditional(ConditionalExpression expression)
        {
            return new JsConditionalExpression(
                Translate(expression.Test),
                Translate(expression.IfTrue),
                Translate(expression.IfFalse));
        }

        public JsExpression TranslateIndex(IndexExpression expression, bool setter = false)
        {
            var target = Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();
            var method = setter ? expression.Indexer.SetMethod : expression.Indexer.GetMethod;

            var result = TryTranslateMethodCall(target, args, method, expression.Object, expression.Arguments.ToArray());
            if (result != null) return result;
            return BuildIndexer(target, args.Single(), expression.Indexer);
        }

        public static JsExpression BuildIndexer(JsExpression target, JsExpression index, MemberInfo member) =>
            target.Indexer(index).WithAnnotation(new VMPropertyInfoAnnotation { MemberInfo = member });

        public JsExpression TranslateParameter(Expression expression, BindingParameterAnnotation annotation)
        {
            JsExpression getDataContext(int parentContexts)
            {
                JsExpression context = new JsSymbolicParameter(KnockoutContextParameter);
                for (var i = 0; i < parentContexts; i++)
                    context = context.Member("$parentContext");
                return context;
            }
            int getContextSteps(DataContextStack item) =>
                item == null ? 0 : ContextMap[item];
            JsExpression contextParameter(string name, int parentContexts, Type type) =>
                getDataContext(parentContexts).Member(name).WithAnnotation(new ViewModelInfoAnnotation(type));

            if (annotation.ExtensionParameter != null)
            {
                return annotation.ExtensionParameter.GetJsTranslation(getDataContext(getContextSteps(annotation.DataContext)));
            }
            else
            {
                var index = getContextSteps(annotation.DataContext);
                if (index == 0)
                    return new JsSymbolicParameter(KnockoutViewModelParameter).WithAnnotation(new ViewModelInfoAnnotation(expression.Type));
                else if (index == 1)
                    return contextParameter("$parent", 0, expression.Type);
                else if (ContextMap.Count == index + 1)
                    return contextParameter("$root", 0, expression.Type);
                else return new JsSymbolicParameter(KnockoutContextParameter)
                        .Member("$parents").Indexer(new JsLiteral(index - 1))
                        .WithAnnotation(new ViewModelInfoAnnotation(expression.Type));
            }
        }

        public JsExpression TranslateParameter(ParameterExpression expression)
        {
            if (WriteUnknownParameters && !string.IsNullOrEmpty(expression.Name)) return new JsIdentifierExpression(expression.Name);
            else throw new NotSupportedException();
        }

        public JsLiteral TranslateConstant(ConstantExpression expression)
        {
            return new JsLiteral(expression.Value);
        }

        public JsExpression TranslateMethodCall(MethodCallExpression expression)
        {
            var thisExpression = expression.Object == null ? null : Translate(expression.Object);
            var args = expression.Arguments.Select(Translate).ToArray();

            if (expression.Method.Name == "GetValue" && expression.Method.DeclaringType == typeof(DotvvmBindableObject))
            {
                var dotvvmproperty = ((DotvvmProperty)((JsLiteral)args[0]).Value);
                return TranslateViewModelProperty(thisExpression, (MemberInfo)dotvvmproperty.PropertyInfo ?? dotvvmproperty.PropertyType.GetTypeInfo(), name: dotvvmproperty.Name);
            }

            var result = TryTranslateMethodCall(thisExpression, args, expression.Method, expression.Object, expression.Arguments.ToArray());
            if (result == null)
                throw new NotSupportedException($"Method { expression.Method.DeclaringType.Name }.{ expression.Method.Name } can not be translated to Javascript");
            return result;
        }

        protected static JsExpression TryTranslateMethodCall(JsExpression context, JsExpression[] args, MethodInfo method, Expression contextExpression, Expression[] argsExpressions) =>
            FindMethodTranslator(method, contextExpression, argsExpressions)?.TranslateCall(context, args, method);

        public static IJsMethodTranslator FindMethodTranslator(MethodInfo method, Expression contextExpression, Expression[] argsExpressions)
        {
            if (method == null) return null;
            if (MethodTranslators.TryGetValue(method, out var translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
            {
                return translator;
            }
            if (method.IsGenericMethod)
            {
                var genericMethod = method.GetGenericMethodDefinition();
                if (MethodTranslators.TryGetValue(genericMethod, out translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
                {
                    return translator;
                }
            }

            foreach (var iface in method.DeclaringType.GetInterfaces())
            {
                if (Interfaces.Contains(iface))
                {
                    var map = method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(iface);
                    var imIndex = Array.IndexOf(map.TargetMethods, method);
                    if (imIndex >= 0 && MethodTranslators.TryGetValue(map.InterfaceMethods[imIndex], out translator) && translator.CanTranslateCall(method, contextExpression, argsExpressions))
                        return translator;
                }
            }
            if (method.DeclaringType.GetTypeInfo().IsGenericType && !method.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericType = method.DeclaringType.GetGenericTypeDefinition();
                var m2 = genericType.GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m2 != null)
                {
                    var r2 = FindMethodTranslator(m2, contextExpression, argsExpressions);
                    if (r2 != null) return r2;
                }
            }
            if (method.DeclaringType == typeof(Array))
            {
                var m2 = typeof(Array).GetMethod(method.Name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                if (m2 != null)
                {
                    var r2 = FindMethodTranslator(m2, contextExpression, argsExpressions);
                    if (r2 != null) return r2;
                }
            }
            var baseMethod = method.GetBaseDefinition();
            if (baseMethod != null && baseMethod != method) return FindMethodTranslator(baseMethod, contextExpression, argsExpressions);
            else return null;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public JsExpression TranslateBinary(BinaryExpression expression)
        {
            var left = Translate(expression.Left);
            var right = Translate(expression.Right);
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
                case ExpressionType.LessThanOrEqual: op = BinaryOperatorType.LessOrEqual; break;
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
            var operand = Translate(expression.Operand);
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
                return TryTranslateMethodCall(Translate(expression.Expression), new JsExpression[0], getter, expression.Expression, new Expression[0]) ??
                    TranslateViewModelProperty(Translate(expression.Expression), expression.Member);
            }
        }

        public JsExpression TranslateViewModelProperty(JsExpression context, MemberInfo propInfo, string name = null)
        {
            //if (propInfo is FieldInfo) throw new NotSupportedException($"Field '{propInfo.Name}' cannot be translated to knockout binding. Use property with public getter and setter.");
            //var protection = propInfo.GetCustomAttribute<ProtectAttribute>();
            //if (protection != null && protection.Settings == ProtectMode.EncryptData)
            //    throw new NotSupportedException($"Encrypted property '{propInfo.Name}' cannot be used in binding.");
            //// Bind(None) can make sense to translate, since it can be used as client-only property
            return new JsMemberAccessExpression(context, name ?? propInfo.Name).WithAnnotation(new VMPropertyInfoAnnotation { MemberInfo = propInfo });
        }
    }

    public class ViewModelInfoAnnotation
    {
        public Type Type { get; set; }
        public bool IsControl { get; set; }

        public ViewModelSerializationMap SerializationMap { get; set; }

        public ViewModelInfoAnnotation(Type type, bool isControl = false)
        {
            this.Type = type;
            this.IsControl = isControl;
        }
    }

    public class VMPropertyInfoAnnotation
    {
        public MemberInfo MemberInfo { get; set; }
        public ViewModelPropertyMap SerializationMap { get; set; }
    }
}