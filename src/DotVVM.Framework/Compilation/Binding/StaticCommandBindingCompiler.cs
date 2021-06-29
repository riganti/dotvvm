using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Compilation.Binding
{
    public class StaticCommandBindingCompiler
    {
        private readonly IViewModelProtector protector;
        private readonly JavascriptTranslator javascriptTranslator;
        public StaticCommandBindingCompiler(JavascriptTranslator javascriptTranslator, IViewModelProtector protector)
        {
            this.protector = protector;
            this.javascriptTranslator = javascriptTranslator;
        }

        /// Replaces delegate arguments with commandArgs reference
        private static Expression ReplaceCommandArgs(Expression expression) =>
            expression.ReplaceAll(e =>
                e?.GetParameterAnnotation()?.ExtensionParameter is TypeConversion.MagicLambdaConversionExtensionParameter extensionParam ?
                    Expression.Parameter(ResolvedTypeDescriptor.ToSystemType(extensionParam.ParameterType), $"commandArgs.['{extensionParam.Identifier}']")
                    .AddParameterAnnotation(new BindingParameterAnnotation(extensionParameter: new JavascriptTranslationVisitor.FakeExtensionParameter(
                        _ => new JsSymbolicParameter(CommandBindingExpression.CommandArgumentsParameter)
                             .Indexer(new JsLiteral(extensionParam.ArgumentIndex))
                    ))) :
                e
            );

        public JsExpression CompileToJavascript(DataContextStack dataContext, Expression expression)
        {
            expression = ReplaceCommandArgs(expression);

            var js = TranslateVariableDeclaration(expression, e => CreateCommandExpression(dataContext, e));

            if (js is JsInvocationExpression invocation && invocation.Target is JsIdentifierExpression identifier && identifier.Identifier == "resolve")
            {
                // optimize `new Promise(function (resolve) { resolve(x) })` to `Promise.resolve(x)`
                identifier.ReplaceWith(new JsIdentifierExpression("Promise").Member("resolve"));
                return js;
            }
            else
            {
                return new JsNewExpression(new JsIdentifierExpression("Promise"), new JsFunctionExpression(
                    new [] { new JsIdentifier("resolve"), new JsIdentifier("reject") },
                    new JsBlockStatement(new JsExpressionStatement(js))
                ));
            }
        }

        private JsExpression TranslateVariableDeclaration(Expression expression, Func<Expression, JsExpression> core)
        {
            expression = VariableHoistingVisitor.HoistVariables(expression);
            if (expression is BlockExpression block && block.Variables.Any())
            {
                var realBlock = block.Update(Enumerable.Empty<ParameterExpression>(), block.Expressions);

                var variables = block.Variables;
                var replacedVariables = ExpressionUtils.Replace(
                    Expression.Lambda(realBlock, variables),
                    variables.Select(v => {
                        var tmpVar = new JsTemporaryVariableParameter();
                        return Expression.Parameter(v.Type, v.Name).AddParameterAnnotation(new BindingParameterAnnotation(extensionParameter:
                            new JavascriptTranslationVisitor.FakeExtensionParameter(_ => new JsSymbolicParameter(tmpVar), v.Name, new ResolvedTypeDescriptor(v.Type))
                        ));
                    }).ToArray()
                );

                return core(replacedVariables);
            }
            else
            {
                return core(expression);
            }
        }

        private JsExpression CreateCommandExpression(DataContextStack dataContext, Expression expression)
        {
            var knockoutContext =
                new JsSymbolicParameter(
                    JavascriptTranslator.KnockoutContextParameter,
                    defaultAssignment: new JsIdentifierExpression("ko").Member("contextFor").Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter)).FormatParametrizedScript()
                );

            var currentContextVariable = new JsTemporaryVariableParameter(knockoutContext);
            // var resultPromiseVariable = new JsNewExpression("DotvvmPromise"));
            var senderVariable = new JsTemporaryVariableParameter(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter));

            var invocationRewriter = new InvocationRewriterExpressionVisitor();
            expression = invocationRewriter.Visit(expression);

            var rewriter = new TaskSequenceRewriterExpressionVisitor();
            expression = rewriter.Visit(expression);

            //Extract all promise returning method calls so that  result can be 'awaited'
            //Result of the call will be saved in auxiliary variable and the wariable is then used in original text
            //example:
            //vm.D = A(B("a"), C("b")) ---> r_0 = B("a"), r_1 = C("b"), r_2 = A(r_0, r_1), vm.D = r_2
            var visitor = new ExtractExpressionVisitor(ex => CreatePromiseMethodCallAnnotationFactory(dataContext, ex));
            var rootCallback = visitor.Visit(expression);

            var errorCallback = new JsIdentifierExpression("reject");
            var js = SouldCompileCallback(rootCallback) ? new JsIdentifierExpression("resolve").Invoke(javascriptTranslator.CompileToJavascript(rootCallback, dataContext)) : null;

            foreach (var param in visitor.ParameterOrder.Reverse<ParameterExpression>())
            {
                js = js ?? new JsIdentifierExpression("resolve").Invoke(new JsIdentifierExpression(param.Name));
                var callback = new JsFunctionExpression(new[] { new JsIdentifier(param.Name) }, new JsBlockStatement(new JsExpressionStatement(js)));
                var method = visitor.Replaced[param] as MethodCallExpression;
                var methodInvocation = CompileMethodCall(method, dataContext, callback, errorCallback.Clone());

                var invocationDependencies = ExtractPostbackCommandInvocationDependecies(methodInvocation);

                var replacedParameterNode = js.DescendantNodes().SingleOrDefault(n => n is JsIdentifierExpression identifier && identifier.Identifier == param.Name);

                //These expressions would be affected by chaging order in which they are executed by putting them in .then(...) callback.
                var orderSensitiveExpressions = ResolveOrderSensitiveExpressions(callback, invocationDependencies, replacedParameterNode);

                if (orderSensitiveExpressions.All(e => e.node is JsExpression))
                {
                    js = methodInvocation;
                    foreach (var (p, node) in Enumerable.Reverse(orderSensitiveExpressions))
                        js = new JsBinaryExpression(
                            new JsAssignmentExpression(new JsSymbolicParameter(p), (JsExpression)node),
                            BinaryOperatorType.Sequence,
                            js
                        );
                }
                else
                {
                    js = JsFunctionExpression.CreateIIFE(
                        new JsBlockStatement(
                            orderSensitiveExpressions.Select(c =>
                                c.parameter == null ? (JsStatement)c.node : new JsExpressionStatement(new JsAssignmentExpression(new JsSymbolicParameter(c.parameter), (JsExpression)c.node))
                            ).Concat(new[] { new JsExpressionStatement(methodInvocation) })
                        )
                    );
                }
            }
            foreach (var sp in js.Descendants.OfType<JsSymbolicParameter>())
            {
                if (sp.Symbol == JavascriptTranslator.KnockoutContextParameter) sp.Symbol = currentContextVariable;
                else if (sp.Symbol == JavascriptTranslator.KnockoutViewModelParameter) sp.ReplaceWith(new JsSymbolicParameter(currentContextVariable).Member("$data"));
                else if (sp.Symbol == CommandBindingExpression.SenderElementParameter) sp.Symbol = senderVariable;
            }
            return js;
        }

        private Func<ParameterExpression, BindingParameterAnnotation> CreatePromiseMethodCallAnnotationFactory(DataContextStack dataContext, Expression ex)
        {
            if (ex.NodeType != ExpressionType.Call || !(ex is MethodCallExpression methodCall)) { return null; }

            if (javascriptTranslator.TryTranslateMethodCall(methodCall.Object, methodCall.Arguments.ToArray(), methodCall.Method, dataContext) is JsExpression jsTranslation)
            {
                if (jsTranslation.Annotation<ResultIsPromiseAnnotation>() is ResultIsPromiseAnnotation promiseAnnotation)
                {
                    return (p => new BindingParameterAnnotation(extensionParameter: new JavascriptTranslationVisitor.FakeExtensionParameter(_ => new JsIdentifierExpression(p.Name).WithAnnotations(promiseAnnotation.ResultAnnotations), p.Name, new ResolvedTypeDescriptor(p.Type))));
                }
                else { return null; }
            }
            return p => new BindingParameterAnnotation(extensionParameter: new JavascriptTranslationVisitor.FakeExtensionParameter(_ => new JsIdentifierExpression(p.Name).WithAnnotation(new ViewModelInfoAnnotation(p.Type)), p.Name, new ResolvedTypeDescriptor(p.Type)));
        }

        private static List<(CodeSymbolicParameter parameter, JsNode node)> ResolveOrderSensitiveExpressions(JsFunctionExpression callback, JsExpression invocationDependencies, JsNode replacedPostbackNode)
        {
            var orderSensitiveExpressions = new List<(CodeSymbolicParameter parameter, JsNode node)>();
            if (replacedPostbackNode != null)
            {
                var siblings = replacedPostbackNode
                    .AncestorsAndSelf.TakeWhile(n => n != callback)
                    .SelectMany(n => n.Parent.Children.TakeWhile(c => c != n))
                    .Select(EnsureInvocationsAndTargetStayTogether)
                    .ToArray();

                //postback(...).then(...)

                var beforePostbackExpressions = new HashSet<JsNode>(siblings);
                beforePostbackExpressions.Add(invocationDependencies);
                foreach (var sibling in siblings)
                {
                    beforePostbackExpressions.Remove(sibling);
                    if (SideEffectAnalyzer.MayReorder(sibling, beforePostbackExpressions))
                        continue;

                    var tmpVar = sibling is JsExpression ? new JsTemporaryVariableParameter() : null;
                    orderSensitiveExpressions.Add((tmpVar, sibling));
                    if (sibling is JsExpression)
                        sibling.ReplaceWith(new JsSymbolicParameter(tmpVar));
                    else if (sibling.Parent is JsBlockStatement)
                        sibling.Remove();
                    else
                        sibling.ReplaceWith(new JsBlockStatement());
                }
            }

            return orderSensitiveExpressions;
        }


        private static JsNode EnsureInvocationsAndTargetStayTogether(JsNode n) =>
            //If I am target of invocation and I am also a member access take my target, otherwise take whole invocation
            //This is so that in cases like: `A(...).B(...)` we  don't split ten experession into `_temp = A(...).B, temp()` doing so would result in invalid this context in javascript
            //in case like `Foo = A(...)` we are better of just taking A. If A is an identifier nothing will change.
            n.Parent is JsInvocationExpression invocation &&
            invocation.Target == n &&
            n is JsMemberAccessExpression targetMemberAccess
            ? targetMemberAccess.Target
            : n;

        private static JsExpression ExtractPostbackCommandInvocationDependecies(JsExpression methodInvocation)
        {
            var commandPostbackInvocation = ExtractCommandPostbackInvocation(methodInvocation);

            var invocationExpressions =
                 commandPostbackInvocation != null ?
                (JsArrayExpression)commandPostbackInvocation.Arguments.ElementAt(2) :
                methodInvocation;
            return invocationExpressions;
        }

        private static JsInvocationExpression ExtractCommandPostbackInvocation(JsExpression methodInvocation) =>
            methodInvocation is JsInvocationExpression thenInvocation &&
            thenInvocation.Target is JsMemberAccessExpression thenMemberAccess &&
            thenMemberAccess.MemberName == "then" &&
            thenMemberAccess.Target is JsInvocationExpression commandInvocation &&
            commandInvocation.Target.ToString() == "dotvvm.staticCommandPostback"
            ? commandInvocation
            : null;

        protected virtual bool SouldCompileCallback(Expression c)
        {
            if (c.NodeType == ExpressionType.Parameter) return false;
            return true;
        }

        protected virtual JsExpression CompileMethodCall(MethodCallExpression methodExpression, DataContextStack dataContext, JsExpression callbackFunction, JsExpression errorCallback)
        {
            var jsTranslation = javascriptTranslator.TryTranslateMethodCall(methodExpression.Object, methodExpression.Arguments.ToArray(), methodExpression.Method, dataContext)
                ?.ApplyAction(javascriptTranslator.AdjustViewModelProperties);
            if (jsTranslation != null)
            {
                if (!(jsTranslation.Annotation<ResultIsPromiseAnnotation>() is ResultIsPromiseAnnotation promiseAnnotation))
                    throw new Exception($"Expected javascript translation that returns a promise");
                var resultPromise = promiseAnnotation.GetPromiseFromExpression?.Invoke(jsTranslation) ?? jsTranslation;
                return resultPromise.Member("then").Invoke(callbackFunction, errorCallback);
            }

            if (!methodExpression.Method.IsDefined(typeof(AllowStaticCommandAttribute)))
                throw new Exception($"Method '{methodExpression.Method.DeclaringType.Name}.{methodExpression.Method.Name}' used in static command has to be marked with [AllowStaticCommand] attribute.");

            if (methodExpression == null) throw new NotSupportedException("Static command binding must be a method call!");

            var (plan, args) = CreateExecutionPlan(methodExpression, dataContext);
            var encryptedPlan = EncryptJson(SerializePlan(plan), protector).Apply(Convert.ToBase64String);

            return new JsIdentifierExpression("dotvvm").Member("staticCommandPostback")
                .Invoke(new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter), new JsLiteral(encryptedPlan), new JsArrayExpression(args), new JsSymbolicParameter(CommandBindingExpression.PostbackOptionsParameter))
                .Member("then")
                .Invoke(callbackFunction, errorCallback)
                .WithAnnotation(new StaticCommandInvocationJsAnnotation(plan));
        }

        public static string[] GetEncryptionPurposes()
        {
            return new[] {
                "StaticCommand",
            };
        }

        private (StaticCommandInvocationPlan plan, JsExpression[] clientArgs) CreateExecutionPlan(MethodCallExpression expression, DataContextStack dataContext)
        {
            var arguments = (expression.Object == null ? new Expression[0] : new[] { expression.Object }).Concat(expression.Arguments).ToArray();
            var clientArgs = new List<JsExpression>();

            var argPlans = arguments.Select((arg, index) => {
                if (arg.GetParameterAnnotation() is BindingParameterAnnotation annotation && annotation.ExtensionParameter is InjectedServiceExtensionParameter service)
                    return new StaticCommandParameterPlan(StaticCommandParameterType.Inject, ResolvedTypeDescriptor.ToSystemType(service.ParameterType));
                else if (arg is ConstantExpression constant)
                {
                    if (constant.Value == expression.Method.GetParameters()[index - (expression.Method.IsStatic ? 0 : 1)].DefaultValue)
                        return new StaticCommandParameterPlan(StaticCommandParameterType.DefaultValue, null);
                    else
                        return new StaticCommandParameterPlan(StaticCommandParameterType.Constant, constant.Value);
                }
                else
                {
                    clientArgs.Add(javascriptTranslator.CompileToJavascript(arg, dataContext));
                    return new StaticCommandParameterPlan(StaticCommandParameterType.Argument, arg.Type);
                }
            }).ToArray();
            return (
                new StaticCommandInvocationPlan(expression.Method, argPlans),
                clientArgs.ToArray()
            );
        }

        public static StaticCommandInvocationPlan DeserializePlan(JToken planInJson)
        {
            var jarray = (JArray)planInJson;
            var typeName = jarray[0].Value<string>();
            var methodName = jarray[1].Value<string>();
            var genericArgumentTypes = jarray[2].Value<JArray>();
            var argTypes = jarray[3].ToObject<byte[]>().Select(a => (StaticCommandParameterType)a).ToArray();

            var methodFound = Type.GetType(typeName).GetMethods()
                .SingleOrDefault(m => m.Name == methodName
                                    && m.GetParameters().Length + (m.IsStatic ? 0 : 1) == argTypes.Length
                                    && m.IsDefined(typeof(AllowStaticCommandAttribute)))
                ?? throw new NotSupportedException($"The specified method was not found.");

            if (methodFound.IsGenericMethod)
            {
                methodFound = methodFound.MakeGenericMethod(
                    genericArgumentTypes.Select(nameToken => Type.GetType(nameToken.Value<string>())).ToArray());
            }

            var methodParameters = methodFound.GetParameters();
            var args = argTypes
                .Select((a, i) => (type: a, arg: jarray.Count <= i + 4 ? JValue.CreateNull() : jarray[i + 4], parameter: (methodFound.IsStatic ? methodParameters[i] : (i == 0 ? null : methodParameters[i - 1]))))
                .Select((a) => {
                    switch (a.type)
                    {
                        case StaticCommandParameterType.Argument:
                        case StaticCommandParameterType.Inject:
                            if (a.arg.Type == JTokenType.Null)
                                return new StaticCommandParameterPlan(a.type, a.parameter?.ParameterType ?? methodFound.DeclaringType);
                            else
                                return new StaticCommandParameterPlan(a.type, a.arg.Value<string>().Apply(Type.GetType));
                        case StaticCommandParameterType.Constant:
                            return new StaticCommandParameterPlan(a.type, a.arg.ToObject(a.parameter?.ParameterType ?? methodFound.DeclaringType));
                        case StaticCommandParameterType.DefaultValue:
                            return new StaticCommandParameterPlan(a.type, a.parameter.DefaultValue);
                        case StaticCommandParameterType.Invocation:
                            return new StaticCommandParameterPlan(a.type, DeserializePlan(a.arg));
                        default:
                            throw new NotSupportedException($"{a.type}");
                    }
                }).ToArray();
            return new StaticCommandInvocationPlan(methodFound, args);
        }

        private static string GetTypeFullName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

        public static JToken SerializePlan(StaticCommandInvocationPlan plan)
        {
            var array = new JArray(
                new JValue(GetTypeFullName(plan.Method.DeclaringType)),
                new JValue(plan.Method.Name),
                new JArray(plan.Method.GetGenericArguments().Select(GetTypeFullName)),
                JToken.FromObject(plan.Arguments.Select(a => (byte)a.Type).ToArray())
            );
            var parameters = (new ParameterInfo[plan.Method.IsStatic ? 0 : 1]).Concat(plan.Method.GetParameters()).ToArray();
            foreach (var (arg, parameter) in plan.Arguments.Zip(parameters, (a, b) => (a, b)))
            {
                if (arg.Type == StaticCommandParameterType.Argument)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType).Equals(arg.Arg))
                        array.Add(JValue.CreateNull());
                    else
                        array.Add(new JValue(arg.Arg.CastTo<Type>().Apply(GetTypeFullName)));
                }
                else if (arg.Type == StaticCommandParameterType.Constant)
                {
                    array.Add(JToken.FromObject(arg.Arg));
                }
                else if (arg.Type == StaticCommandParameterType.DefaultValue)
                {
                    array.Add(JValue.CreateNull());
                }
                else if (arg.Type == StaticCommandParameterType.Inject)
                {
                    if ((parameter?.ParameterType ?? plan.Method.DeclaringType).Equals(arg.Arg))
                        array.Add(JValue.CreateNull());
                    else
                        array.Add(new JValue(arg.Arg.CastTo<Type>().Apply(GetTypeFullName)));
                }
                else if (arg.Type == StaticCommandParameterType.Invocation)
                {
                    array.Add(SerializePlan((StaticCommandInvocationPlan)arg.Arg));
                }
                else throw new NotSupportedException(arg.Type.ToString());
            }
            while (array.Last.Type == JTokenType.Null)
                array.Last.Remove();
            return array;
        }

        public static byte[] EncryptJson(JToken json, IViewModelProtector protector)
        {
            var stream = new MemoryStream();
            using (var writer = new JsonTextWriter(new StreamWriter(stream)))
            {
                json.WriteTo(writer);
            }
            return protector.Protect(stream.ToArray(), GetEncryptionPurposes());
        }

        public static JToken DecryptJson(byte[] data, IViewModelProtector protector)
        {
            using (var reader = new JsonTextReader(new StreamReader(new MemoryStream(protector.Unprotect(data, GetEncryptionPurposes())))))
            {
                return JToken.ReadFrom(reader);
            }
        }

        public sealed class StaticCommandInvocationJsAnnotation
        {
            public MethodInfo Method => Plan.Method;
            public StaticCommandInvocationPlan Plan { get; }
            public StaticCommandInvocationJsAnnotation(StaticCommandInvocationPlan plan)
            {
                this.Plan = plan;
            }
        }
    }

    public class StaticCommandInvocationPlan
    {
        public MethodInfo Method { get; }
        public StaticCommandParameterPlan[] Arguments { get; }
        public StaticCommandInvocationPlan(MethodInfo method, StaticCommandParameterPlan[] args)
        {
            this.Method = method;
            this.Arguments = args;
        }

        public IEnumerable<MethodInfo> GetAllMethods()
        {
            yield return Method;
            foreach (var arg in Arguments)
                if (arg.Type == StaticCommandParameterType.Invocation)
                    foreach (var r in arg.Arg.CastTo<StaticCommandInvocationPlan>().GetAllMethods())
                        yield return r;
        }
    }

    public class StaticCommandParameterPlan
    {
        public StaticCommandParameterType Type { get; }
        public object Arg { get; }

        public StaticCommandParameterPlan(StaticCommandParameterType type, object arg)
        {
            this.Type = type;
            this.Arg = arg;
        }
    }
    public enum StaticCommandParameterType : byte
    {
        Argument,
        Inject,
        Constant,
        DefaultValue,
        Invocation
    }
}
