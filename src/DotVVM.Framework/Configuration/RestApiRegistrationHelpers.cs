using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Binding.Properties;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public static class RestApiRegistrationHelpers
    {
        static RestApiRegistrationHelpers()
        {
            //JavascriptTranslator.AddPropertyGetterTranslator()
        }

        private static HashSet<(DotvvmConfiguration, Type)> apiDtosProcessed = new HashSet<(DotvvmConfiguration, Type)>();
        private static ConditionalWeakTable<DotvvmConfiguration, HashSet<Type>> apiClientProcessed = new ConditionalWeakTable<DotvvmConfiguration, HashSet<Type>>();
        private static object locker = new object();
        private static void RegisterApiDtoProperties(Type obj, DotvvmConfiguration config, Assembly currentAssembly = null)
        {
            currentAssembly = currentAssembly ?? obj.Assembly;
            bool isSameAssembly(Type type) => type.Assembly == currentAssembly || type.GetGenericArguments().Any(isSameAssembly);
            lock (locker)
            {
                if (!apiDtosProcessed.Add((config, obj))) return;

                if (obj.Assembly != typeof(string).Assembly &&
                    !obj.IsGenericParameter &&
                    ViewModelJsonConverter.CanConvertType(obj))
                {

                    config.GetSerializationMapper().Map(obj, m => {
                        foreach (var prop in m.Properties)
                        {
                            if (isSameAssembly(prop.Type))
                                RegisterApiDtoProperties(prop.Type, config, currentAssembly);
                        }
                    });
                }

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(obj) && ReflectionUtils.GetEnumerableType(obj) is Type element && isSameAssembly(element))
                    RegisterApiDtoProperties(element, config, currentAssembly);

                foreach (var t in obj.GenericTypeArguments)
                    if (isSameAssembly(t))
                        RegisterApiDtoProperties(t, config, currentAssembly);
            }
        }

        private static JsExpression[] SerializeComplexParameters(JsExpression[] expr)
        {
            return expr.Select(p =>
                p.Annotation<ViewModelInfoAnnotation>() is ViewModelInfoAnnotation vmInfo ? (
                    ReflectionUtils.IsComplexType(vmInfo.Type) ? Serialize(p) :
                    vmInfo.Type == typeof(DateTime) || vmInfo.Type == typeof(DateTime?) ? SerializeDate(p) :
                    p) :
                p
            ).ToArray();
        }

        private static JsExpression Serialize(JsExpression expr) =>
            new JsIdentifierExpression("dotvvm").Member("serialization").Member("serialize").Invoke(expr.WithAnnotation(ShouldBeObservableAnnotation.Instance));
        
        private static JsExpression SerializeDate(JsExpression expr) =>
            new JsIdentifierExpression("dotvvm").Member("serialization").Member("parseDate").Invoke(expr, new JsLiteral(true));

        private static JsExpression[] ReplaceDefaultWithUndefined(IEnumerable<JsExpression> arguments, ParameterInfo[] parameters)
        {
            var replaced = arguments.Zip(parameters, (a, p) => a is JsLiteral literal && literal.Value == p.DefaultValue ? new JsIdentifierExpression("undefined") : a).ToArray();
            int trimCount = 0;
            while (trimCount < replaced.Length && replaced[replaced.Length - trimCount - 1] is JsIdentifierExpression identifier && identifier.Identifier == "undefined")
                trimCount++;
            return replaced.Take(replaced.Length - trimCount).ToArray();
        }

        private static void RegisterJsTranslation(JsExpression identifier, Type apiClient, DotvvmConfiguration config)
        {
            lock (locker)
            {
                var registerJS = apiClientProcessed.GetOrCreateValue(config).Add(apiClient);

                foreach (var method in apiClient.GetMethods())
                {
                    if (typeof(Task).IsAssignableFrom(method.ReturnType) || method.IsSpecialName) continue;

                    RegisterApiDtoProperties(method.ReturnType, config, method.DeclaringType.Assembly);
                    foreach (var p in method.GetParameters())
                        RegisterApiDtoProperties(p.ParameterType, config, method.DeclaringType.Assembly);

                    if (registerJS)
                    {
                        var isRead = IsHttpReadMethod(method);

                        // build unique method identifier for GET request sharing purposes
                        var sharingKey = GetApiMethodUrl(identifier.FormatScript(), method);
                        var sharingKeyExpression = BuildMethodSharingKeyExpression(sharingKey, method);

                        // get auto-refresh identifier
                        var autoRefreshKey = GetAutoRefreshKey(apiClient, method);
                        var autoRefreshKeyExpression = new JsLiteral(autoRefreshKey);

                        config.Markup.JavascriptTranslator.MethodCollection.AddMethodTranslator(method, new GenericMethodCompiler(
                            a => new JsIdentifierExpression("dotvvm").Member("api").Member("invoke").Invoke(
                                identifier.Clone(),
                                new JsLiteral(KnockoutHelper.ConvertToCamelCase(method.Name)),
                                new JsFunctionExpression(new JsIdentifier[0], new JsBlockStatement(new JsReturnStatement(
                                        new JsArrayExpression(ReplaceDefaultWithUndefined(a.Skip(1), method.GetParameters()).Apply(SerializeComplexParameters))
                                ))),
                                new JsFunctionExpression(new[] { new JsIdentifier("args") }, new JsBlockStatement(new JsReturnStatement(
                                    new JsArrayExpression(isRead ? new JsIdentifierExpression("dotvvm").Member("eventHub").Member("get").Invoke(autoRefreshKeyExpression.Clone()) : null)
                                ))),
                                new JsFunctionExpression(new[] { new JsIdentifier("args") }, new JsBlockStatement(new JsReturnStatement(
                                    new JsArrayExpression(!isRead ? autoRefreshKeyExpression.Clone() : null)
                                ))),
                                isRead ? (JsExpression)new JsLiteral(null) : new JsSymbolicParameter(CommandBindingExpression.SenderElementParameter, new CodeParameterAssignment("$element", OperatorPrecedence.Max)),
                                new JsFunctionExpression(new[] { new JsIdentifier("args") }, new JsBlockStatement(new JsReturnStatement(
                                    isRead ? sharingKeyExpression.Clone() : new JsLiteral(Guid.NewGuid().ToString())
                                )))
                            ).WithAnnotation(ResultIsObservableAnnotation.Instance)
                             .WithAnnotation(MayBeNullAnnotation.Instance)
                             .WithAnnotation(new ViewModelInfoAnnotation(method.ReturnType))
                             .WithAnnotation(new ResultIsPromiseAnnotation(
                                 e => e.WithAnnotation(ShouldBeObservableAnnotation.Instance).Member("refreshValue").Invoke(new JsLiteral(true)),
                                 new ViewModelInfoAnnotation(method.ReturnType, containsObservables: false)
                             ) { IsOptionalAwait = true })
                        ));
                    }
                }
            }
        }

        private static string GetAutoRefreshKey(Type apiClient, MethodInfo method)
        {
            return apiClient.FullName + "/" + method.GetCustomAttribute<AutoRefreshKeyAttribute>()?.Key ?? "";
        }

        private static JsExpression BuildMethodSharingKeyExpression(string sharingKey, MethodInfo method)
        {
            // takes an URL pattern (e.g. /api/customer/{id}) and generates an JS expression that substitutes the parameters with actual values from the args[] array
            JsExpression sharingKeyExpression = new JsLiteral(sharingKey);
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (sharingKey.Contains("{" + parameters[i].Name + "}"))
                {
                    sharingKeyExpression = sharingKeyExpression.Member("replace").Invoke(
                        new JsLiteral("{" + parameters[i].Name + "}"),
                        new JsIdentifierExpression("args").Indexer(new JsLiteral(i))
                    );
                }
            }

            return sharingKeyExpression;
        }

        public static void RegisterApiClient(this DotvvmConfiguration configuration, Type clientType, string apiServerUrl, string jsApiClientFile, string identifier, string customFetchFunction = null)
        {
            apiServerUrl = apiServerUrl.TrimEnd('/');
            var jsidentifier = new JsIdentifierExpression("dotvvm").Member("api").Member(identifier);
            var jsinitializer = new JsExpressionStatement(new JsAssignmentExpression(
                jsidentifier.Clone(),
                new JsNewExpression(
                    new JsIdentifierExpression(clientType.FullName),
                    new JsLiteral(apiServerUrl),
                    CreateHttpObj(customFetchFunction)
                )
            ));
            var instance = CreateApiClientInstance(apiServerUrl, clientType);
            var descriptor = new ApiGroupDescriptor(clientType, new [] { new ApiDescriptor(null, null, clientType, jsidentifier) }, instance);
            RegisterApiDependencies(configuration, identifier, jsApiClientFile, jsinitializer, descriptor);
        }

        public static void RegisterApiGroup(this DotvvmConfiguration configuration, Type wrapperType, string apiServerUrl, string jsApiClientFile, string identifier = "_api", string customFetchFunction = null)
        {
            apiServerUrl = apiServerUrl.TrimEnd('/');
            var jsidentifier = new JsIdentifierExpression("dotvvm").Member("api").Member(identifier);

            var properties = (from prop in wrapperType.GetProperties()
                              let instance = CreateApiClientInstance(apiServerUrl, prop.PropertyType)
                              let jsName = KnockoutHelper.ConvertToCamelCase(prop.Name)
                              select new { instance, jsName, desc = new ApiDescriptor(prop.Name, prop, prop.PropertyType, jsidentifier.Clone().Member(jsName)) }).ToArray();

            var jsinitializer = new JsExpressionStatement(new JsAssignmentExpression(jsidentifier.Clone(), new JsObjectExpression(
                properties.Select(p =>
                    new JsObjectProperty(p.jsName, new JsNewExpression(new JsIdentifierExpression(p.desc.Type.FullName),
                        new JsLiteral(apiServerUrl),
                        CreateHttpObj(customFetchFunction)
                    ))
                )
            )));

            var wrapperInstance = Activator.CreateInstance(wrapperType);
            foreach (var prop in properties)
                prop.desc.PropInfo.SetValue(wrapperInstance, prop.instance);

            var descriptor = new ApiGroupDescriptor(wrapperType, properties.Select(p => p.desc), wrapperInstance);
            RegisterApiDependencies(configuration, identifier, jsApiClientFile, jsinitializer, descriptor);
        }

        private static object CreateApiClientInstance(string apiServerUrl, Type type)
        {
            return type.GetConstructor(new[] { typeof(string) })?.Invoke(new[] { apiServerUrl }) ??
                   type.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
        }

        private static JsObjectExpression CreateHttpObj(string customFetchFunction)
        {
            return customFetchFunction == null ? null : new JsObjectExpression(new JsObjectProperty("fetch", new JsIdentifierExpression(customFetchFunction)));
        }

        private static void RegisterApiDependencies(DotvvmConfiguration configuration, string identifier, string jsApiClientFile, JsNode jsinitializer, ApiGroupDescriptor descriptor)
        {
            configuration.Resources.Register("apiClient" + identifier, new ScriptResource(location: new FileResourceLocation(jsApiClientFile)));
            configuration.Resources.Register("apiInit" + identifier, new InlineScriptResource(defer: true, code: jsinitializer.FormatScript(niceMode: configuration.Debug)) { Dependencies = new[] { "dotvvm", "apiClient" + identifier } });

            configuration.Markup.DefaultExtensionParameters.Add(new ApiExtensionParameter(identifier, descriptor));

            foreach (var prop in descriptor.Properties)
            {
                prop.JsExpression.AddAnnotation(new RequiredRuntimeResourcesBindingProperty(ImmutableArray.Create("apiInit" + identifier)));
                RegisterJsTranslation(prop.JsExpression, prop.Type, configuration);
            }
        }

        private const string HttpGetVerb = "Get";
        private const string HttpHeadVerb = "Head";
        private static bool IsHttpReadMethod(MethodInfo method)
        {
            var httpMethod = method.GetCustomAttribute<HttpMethodAttribute>()?.Method;

            if (httpMethod != null)
            {
                return httpMethod.Equals(HttpGetVerb, StringComparison.OrdinalIgnoreCase)
                    || httpMethod.Equals(HttpHeadVerb, StringComparison.OrdinalIgnoreCase);
            }

            return method.Name.StartsWith(HttpGetVerb, StringComparison.OrdinalIgnoreCase)
                || method.Name.StartsWith(HttpHeadVerb, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetApiMethodUrl(string identifier, MethodInfo method)
        {
            return method.GetCustomAttribute<MethodUrlAttribute>()?.Url ?? "";
        }

        public class ApiGroupDescriptor
        {
            public object Instance { get; }
            public ImmutableArray<ApiDescriptor> Properties { get; }
            public Type Type { get; }
            public bool IsSingleClient => Properties.Length == 1 && Properties.Single().Name == null;

            public ApiGroupDescriptor(Type type, IEnumerable<ApiDescriptor> properties, object instance)
            {
                this.Type = type;
                this.Properties = properties.ToImmutableArray();
                this.Instance = instance;
            }
        }

        public class ApiDescriptor
        {
            public string Name { get; }
            public PropertyInfo PropInfo { get; }
            public Type Type { get; }
            public JsExpression JsExpression { get; }

            public ApiDescriptor(string name, PropertyInfo propInfo, Type type, JsExpression expression)
            {
                this.Name = name;
                this.PropInfo = propInfo;
                this.Type = type;
                expression.Freeze();
                this.JsExpression = expression;
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class HttpMethodAttribute : Attribute
        {
            public HttpMethodAttribute(string method)
            {
                Method = method;
            }

            public string Method { get; }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class MethodUrlAttribute : Attribute
        {
            public MethodUrlAttribute(string url)
            {
                Url = url;
            }

            public string Url { get; }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        public sealed class AutoRefreshKeyAttribute : Attribute
        {
            public AutoRefreshKeyAttribute(string key)
            {
                Key = key;
            }

            public string Key { get; }
        }

        public class ApiExtensionParameter : BindingExtensionParameter
        {
            public ApiExtensionParameter(string identifier, ApiGroupDescriptor descriptor) : base(identifier, new ResolvedTypeDescriptor(descriptor.Type), inherit: true)
            {
                this.ApiDescriptor = descriptor;
            }

            [JsonIgnore]
            public ApiGroupDescriptor ApiDescriptor { get; }

            public override JsExpression GetJsTranslation(JsExpression dataContext) =>
                this.ApiDescriptor.IsSingleClient ?
                this.ApiDescriptor.Properties.Single().JsExpression.Clone() :
                new JsObjectExpression(
                    ApiDescriptor.Properties.Select(p => new JsObjectProperty(p.Name, p.JsExpression.Clone()))
                );

            public override Expression GetServerEquivalent(Expression controlParameter) =>
                Expression.Constant(null, ApiDescriptor.Type);
        }
    }
}
