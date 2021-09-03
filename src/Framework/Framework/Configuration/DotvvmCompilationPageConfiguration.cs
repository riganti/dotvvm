using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json;

namespace DotVVM.Framework.Configuration
{
    public class DotvvmCompilationPageConfiguration
    {
        public const string DefaultUrl = "_dotvvm/diagnostics/compilation";
        public const string DefaultRouteName = "_dotvvm_diagnostics_compilation";

        /// <summary>
        /// Gets or sets whether the compilation status page is enabled.
        /// </summary>
        /// <remarks>
        /// When null, the compilation page is automatically enabled if <see cref="DotvvmConfiguration.Debug"/>
        /// is true.
        /// </remarks>
        [JsonProperty("isEnabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public bool? IsEnabled
        {
            get { return _isEnabled; }
            set { ThrowIfFrozen(); _isEnabled = value; }
        }
        private bool? _isEnabled = null;

        /// <summary>
        /// Gets or sets whether the compilation status page API is enabled.
        /// </summary>
        /// <remarks>
        /// If set to true, an additional route named {RouteName}Api will be registered at
        /// {Url}/api. If a GET request is sent to this route, an HTTP 200 status code is returned
        /// if all pages and controls can be compiled successfully, otherwise an HTTP 500 status code
        /// is sent back.
        /// </remarks>
        [JsonProperty("isApiEnabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(false)]
        public bool IsApiEnabled
        {
            get { return _isApiEnabled; }
            set { ThrowIfFrozen(); _isApiEnabled = value; }
        }
        private bool _isApiEnabled = false;

        /// <summary>
        /// Gets or sets the URL where the compilation page will be accessible from.
        /// </summary>
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(DefaultUrl)]
        public string Url
        {
            get { return _url; }
            set { ThrowIfFrozen(); _url = value; }
        }
        private string _url = DefaultUrl;

        /// <summary>
        /// Gets or sets the name of the route that the compilation page will be registered as.
        /// </summary>
        [JsonProperty("routeName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(DefaultRouteName)]
        public string RouteName
        {
            get { return _routeName; }
            set { ThrowIfFrozen(); _routeName = value; }
        }
        private string _routeName = DefaultRouteName;


        /// <summary>
        /// Gets or sets whether the compilation page should attempt to compile all registered
        /// pages and markup controls when it is loaded.
        /// </summary>
        [JsonProperty("shouldCompileAllOnLoad", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool ShouldCompileAllOnLoad
        {
            get { return _shouldCompileAllOnLoad; }
            set { ThrowIfFrozen(); _shouldCompileAllOnLoad = value; }
        }
        private bool _shouldCompileAllOnLoad = true;

        /// <summary>
        /// Gets or sets a predicate used to check that a request to the compilation page
        /// or the API (enabled by <see cref="IsApiEnabled"/>) is authorized.
        /// </summary>
        /// <remarks>
        /// By default, only local requests are authorized.
        /// </remarks>
        [JsonIgnore]
        public Func<IDotvvmRequestContext, Task<bool>> AuthorizationPredicate
        {
            get { return _authorizationPredicate; }
            set { ThrowIfFrozen(); _authorizationPredicate = value; }
        }
        private Func<IDotvvmRequestContext, Task<bool>> _authorizationPredicate
            = context => Task.FromResult(context.HttpContext.Request.Url.IsLoopback);

        private bool isFrozen = false;

        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(DotvvmCompilationPageConfiguration));
        }

        public void Freeze()
        {
            isFrozen = true;
        }

        public void Apply(DotvvmConfiguration config)
        {
            if (IsEnabled == true || (IsEnabled == null && config.Debug))
            {
                config.RouteTable.Add(
                    routeName: RouteName,
                    url: Url,
                    virtualPath: "embedded://DotVVM.Framework/Diagnostics/CompilationPage.dothtml");
            }

            if (IsApiEnabled)
            {
                config.RouteTable.Add(
                    routeName: $"{RouteName}_api",
                    url: $"{Url}/api",
                    presenterType: typeof(CompilationPageApiPresenter));
            }
        }
    }
}
