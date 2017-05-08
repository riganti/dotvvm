using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Linq.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Configuration
{
    public static class RestApiRegistrationHelpers
    {
        static RestApiRegistrationHelpers()
        {
            //JavascriptTranslator.AddPropertyGetterTranslator()
        }

        private static HashSet<(DotvvmConfiguration, Type)> apiDtosProcessed = new HashSet<(DotvvmConfiguration, Type)>();
        private static HashSet<Type> apiClientProcessed = new HashSet<Type>();
        private static object locker = new object();
        private static void RegisterApiDtoProperties(Type obj, DotvvmConfiguration config)
        {
            lock (locker)
            {
                if (!apiDtosProcessed.Add((config, obj))) return;

                if (obj.GetTypeInfo().Assembly != typeof(string).GetTypeInfo().Assembly &&
                    !obj.IsGenericParameter &&
                    !ViewModelJsonConverter.IsEnumerable(obj) && ViewModelJsonConverter.IsComplexType(obj) && !ViewModelJsonConverter.IsTuple(obj))
                {

                    config.ServiceLocator.GetService<IViewModelSerializationMapper>().Map(obj, m => {
                        foreach (var prop in m.Properties)
                            prop.Name = KnockoutHelper.ConvertToCamelCase(prop.Name);
                    });
                }

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(obj) && ReflectionUtils.GetEnumerableType(obj) is Type element)
                    RegisterApiDtoProperties(element, config);
                foreach (var t in obj.GenericTypeArguments)
                    RegisterApiDtoProperties(t, config);
            }
        }
        private static void RegisterJsTranslation(JsExpression identifier, Type apiClient, DotvvmConfiguration config)
        {
            lock (locker)
            {
                var registerJS = apiClientProcessed.Add(apiClient);

                foreach (var method in apiClient.GetMethods())
                {
                    if (typeof(Task).IsAssignableFrom(method.ReturnType) || method.IsSpecialName) continue;

                    RegisterApiDtoProperties(method.ReturnType, config);
                    foreach (var p in method.GetParameters())
                        RegisterApiDtoProperties(p.ParameterType, config);

                    if (registerJS) JavascriptTranslator.AddMethodTranslator(method, new GenericMethodCompiler(
                            a => new JsIdentifierExpression("dotvvm").Member("invokeApiFn").Invoke(
                                new JsFunctionExpression(new JsIdentifier[0], new JsBlockStatement(
                                    new JsReturnStatement(identifier.Clone().Member(KnockoutHelper.ConvertToCamelCase(method.Name)).Invoke(a.Skip(1).ToArray()))
                                )),
                                new JsArrayExpression(new JsIdentifierExpression("dotvvm").Member("eventHub").Member("get").Invoke(new JsLiteral(identifier.FormatScript())))
                            )
                        ));
                }
            }
        }

        public static void RegisterApiGroup(this DotvvmConfiguration configuration, Type wrapperType, string apiServerUrl, string jsApiClientFile, string identifier = "_api")
        {
            apiServerUrl = apiServerUrl.TrimEnd('/');
            var jsidentifier = new JsIdentifierExpression("dotvvm").Member("api").Member(identifier);

            var properties = (from prop in wrapperType.GetProperties()
                              let ctor = prop.PropertyType.GetConstructor(new[] { typeof(string) })
                              let instance = ctor.Invoke(new[] { apiServerUrl })
                              let jsName = KnockoutHelper.ConvertToCamelCase(prop.Name)
                              select new { instance, jsName, desc = new ApiDescriptor(prop.Name, prop, prop.PropertyType, jsidentifier.Clone().Member(jsName)) }).ToArray();

            var jsinitializer = new JsExpressionStatement(new JsAssignmentExpression(jsidentifier.Clone(), new JsObjectExpression(
                properties.Select(p =>
                    new JsObjectProperty(p.jsName, new JsNewExpression(new JsIdentifierExpression(p.desc.Type.Name), new JsLiteral(apiServerUrl)))
                )
            )));

            configuration.Resources.Register("apiClient" + identifier, new ScriptResource(new FileResourceLocation(jsApiClientFile)));
            configuration.Resources.Register("apiInit" + identifier, new InlineScriptResource(jsinitializer.FormatScript(niceMode: configuration.Debug)) { Dependencies = new[] { "dotvvm", "apiClient" + identifier } });

            var wrapperInstance = Activator.CreateInstance(wrapperType);
            foreach (var prop in properties)
                prop.desc.PropInfo.SetValue(wrapperInstance, prop.instance);

            var descriptor = new ApiGroupDescriptor(wrapperType, properties.Select(p => p.desc), wrapperInstance);

            configuration.Markup.DefaultExtensionParameters.Add(new ApiExtensionParameter(identifier, descriptor));

            foreach (var prop in properties)
                RegisterJsTranslation(prop.desc.JsExpression, prop.desc.Type, configuration);
        }

        class ApiGroupDescriptor
        {
            public object Instance { get; }
            public ImmutableArray<ApiDescriptor> Properties { get; }
            public Type Type { get; }

            public ApiGroupDescriptor(Type type, IEnumerable<ApiDescriptor> properties, object instance)
            {
                this.Type = type;
                this.Properties = properties.ToImmutableArray();
                this.Instance = instance;
            }
        }

        class ApiDescriptor
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

        class ApiExtensionParameter : BindingExtensionParameter
        {
            public ApiExtensionParameter(string identifier, ApiGroupDescriptor descriptor) : base(identifier, new ResolvedTypeDescriptor(descriptor.Type), inherit: true)
            {
                this.ApiDescriptor = descriptor;
            }

            public ApiGroupDescriptor ApiDescriptor { get; }

            public override JsExpression GetJsTranslation(JsExpression dataContext) =>
                new JsObjectExpression(
                    ApiDescriptor.Properties.Select(p => new JsObjectProperty(p.Name, p.JsExpression.Clone()))
                );

            public override Expression GetServerEquivalent(Expression controlParameter) =>
                Expression.Constant(ApiDescriptor.Instance, ApiDescriptor.Type);
        }
    }
}
