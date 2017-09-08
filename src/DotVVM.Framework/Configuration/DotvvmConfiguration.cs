using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using Newtonsoft.Json;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using System.Globalization;
using System.Reflection;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Runtime.Tracing;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmConfiguration
    {
        public const string DotvvmControlTagPrefix = "dot";

        /// <summary>
        /// Gets or sets the application physical path.
        /// </summary>
        [JsonIgnore]
        public string ApplicationPhysicalPath { get; set; }

        /// <summary>
        /// Gets the settings of the markup.
        /// </summary>
        [JsonProperty("markup", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmMarkupConfiguration Markup { get; private set; }

        /// <summary>
        /// Gets the route table.
        /// </summary>
        [JsonProperty("routes")]
        [JsonConverter(typeof(RouteTableJsonConverter))]
        public DotvvmRouteTable RouteTable { get; private set; }

        /// <summary>
        /// Gets the configuration of resources.
        /// </summary>
        [JsonProperty("resources", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(ResourceRepositoryJsonConverter))]
        public DotvvmResourceRepository Resources { get; private set; }

        /// <summary>
        /// Gets the security configuration.
        /// </summary>
        [JsonProperty("security")]
        public DotvvmSecurityConfiguration Security { get; private set; }

        /// <summary>
        /// Gets the runtime configuration.
        /// </summary>
        [JsonProperty("runtime", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DotvvmRuntimeConfiguration Runtime { get; private set; }

        /// <summary>
        /// Gets or sets the default culture.
        /// </summary>
        [JsonProperty("defaultCulture", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets whether the client side validation rules should be enabled.
        /// </summary>
        [JsonProperty("clientSideValidation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool ClientSideValidation { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the application should run in debug mode.
        /// </summary>
        [JsonProperty("debug", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Debug { get; set; }

        [JsonIgnore]
        public Dictionary<string, IRouteParameterConstraint> RouteConstraints { get; } = new Dictionary<string, IRouteParameterConstraint>();

        /// <summary>
        /// Whether DotVVM compiler should generate runtime debug info for bindings. It can be useful, but may also cause unexpected problems.
        /// </summary>
        public bool AllowBindingDebugging { get; set; }

        /// <summary>
        /// Gets an instance of the service locator component.
        /// </summary>
        [JsonIgnore]
        public ServiceLocator ServiceLocator { get; private set; }

        [JsonIgnore]
        public StyleRepository Styles { get; set; }

        [JsonProperty("compiledViewsAssemblies")]
        public List<string> CompiledViewsAssemblies { get; set; } = new List<string>() { "CompiledViews.dll" };

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmConfiguration"/> class.
        /// </summary>
        internal DotvvmConfiguration()
        {
            DefaultCulture = CultureInfo.CurrentCulture.Name;
            Markup = new DotvvmMarkupConfiguration();
            RouteTable = new DotvvmRouteTable(this);
            Resources = new DotvvmResourceRepository();
            Security = new DotvvmSecurityConfiguration();
            Runtime = new DotvvmRuntimeConfiguration();
            Styles = new StyleRepository();
        }

        /// <summary>
        /// Creates the default configuration and optionally registers additional application services.
        /// </summary>
        /// <param name="registerServices">An action to register additional services.</param>
        public static DotvvmConfiguration CreateDefault(Action<IServiceCollection> registerServices = null)
        {
            var services = new ServiceCollection();
            var config = CreateDefault(new ServiceLocator(services));

            DotvvmServiceCollectionExtensions.RegisterDotVVMServices(services, config);
            registerServices?.Invoke(services);

            return config;
        }

        /// <summary>
        /// Creates the default configuration using the given service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve services from.</param>
        public static DotvvmConfiguration CreateDefault(IServiceProvider serviceProvider)
            => CreateDefault(new ServiceLocator(serviceProvider));

        private static DotvvmConfiguration CreateDefault(ServiceLocator serviceLocator)
        {
            var config = new DotvvmConfiguration
            {
                ServiceLocator = serviceLocator
            };

            config.Runtime.GlobalFilters.Add(new ModelValidationFilterAttribute());

            config.Markup.Controls.AddRange(new[]
            {
                new DotvvmControlConfiguration() { TagPrefix = "dot", Namespace = "DotVVM.Framework.Controls", Assembly = "DotVVM.Framework" }
            });

            RegisterConstraints(config);
            RegisterResources(config);

            return config;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        static void RegisterConstraints(DotvvmConfiguration configuration)
        {
            configuration.RouteConstraints.Add("alpha", GenericRouteParameterType.Create("[a-zA-Z]*?"));
            configuration.RouteConstraints.Add("bool", GenericRouteParameterType.Create<bool>("true|false", bool.TryParse));
            configuration.RouteConstraints.Add("decimal", GenericRouteParameterType.Create<decimal>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("double", GenericRouteParameterType.Create<double>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("float", GenericRouteParameterType.Create<float>("-?[0-9.e]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("guid", GenericRouteParameterType.Create<Guid>("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", Guid.TryParse));
            configuration.RouteConstraints.Add("int", GenericRouteParameterType.Create<int>("-?[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("posint", GenericRouteParameterType.Create<int>("[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("length", new GenericRouteParameterType(p => "[^/]{" + p + "}"));
            configuration.RouteConstraints.Add("long", GenericRouteParameterType.Create<long>("-?[0-9]*?", Invariant.TryParse));
            configuration.RouteConstraints.Add("max", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) =>
            {
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                if (double.Parse(parameter, CultureInfo.InvariantCulture) < value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("min", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) =>
            {
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                if (double.Parse(parameter, CultureInfo.InvariantCulture) > value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("range", new GenericRouteParameterType(p => "-?[0-9.e]*?", (valueString, parameter) =>
            {
                double value;
                if (!Invariant.TryParse(valueString, out value)) return ParameterParseResult.Failed;
                var split = parameter.Split(',');
                if (double.Parse(split[0], CultureInfo.InvariantCulture) > value || double.Parse(split[1], CultureInfo.InvariantCulture) < value) return ParameterParseResult.Failed;
                return ParameterParseResult.Create(value);
            }));
            configuration.RouteConstraints.Add("maxLength", new GenericRouteParameterType(p => "[^/]{0," + p + "}"));
            configuration.RouteConstraints.Add("minLength", new GenericRouteParameterType(p => "[^/]{" + p + ",}"));
            configuration.RouteConstraints.Add("regex", new GenericRouteParameterType(p => p));
        }

        private static void RegisterResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.KnockoutJSResourceName,
                new ScriptResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).GetTypeInfo().Assembly,
                    "DotVVM.Framework.Resources.Scripts.knockout-latest.js")));

            configuration.Resources.Register(ResourceConstants.DotvvmResourceName + ".internal",
                new ScriptResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).GetTypeInfo().Assembly,
                    "DotVVM.Framework.Resources.Scripts.DotVVM.js"))
                {
                    Dependencies = new[] { ResourceConstants.KnockoutJSResourceName }
                });
            configuration.Resources.Register(ResourceConstants.DotvvmResourceName,
                new InlineScriptResource()
                {
                    Code = @"if (window.dotvvm) { throw 'DotVVM is already loaded!'; } window.dotvvm = new DotVVM();",
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName + ".internal" }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmDebugResourceName,
                new ScriptResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).GetTypeInfo().Assembly,
                    "DotVVM.Framework.Resources.Scripts.DotVVM.Debug.js"))
                {
                    Dependencies = new[] { ResourceConstants.DotvvmResourceName }
                });

            configuration.Resources.Register(ResourceConstants.DotvvmFileUploadCssResourceName,
                new StylesheetResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).GetTypeInfo().Assembly,
                    "DotVVM.Framework.Resources.Scripts.DotVVM.FileUpload.css")));

            RegisterGlobalizeResources(configuration);
        }

        private static void RegisterGlobalizeResources(DotvvmConfiguration configuration)
        {
            configuration.Resources.Register(ResourceConstants.GlobalizeResourceName,
                new ScriptResource(new EmbeddedResourceLocation(
                    typeof(DotvvmConfiguration).GetTypeInfo().Assembly,
                    "DotVVM.Framework.Resources.Scripts.Globalize.globalize.js")));

            configuration.Resources.RegisterNamedParent("globalize", new JQueryGlobalizeResourceRepository());
        }

    }
}
