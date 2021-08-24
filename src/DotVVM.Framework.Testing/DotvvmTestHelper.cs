using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Tokenizer;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Caching;
using DotVVM.Framework.Security;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DotVVM.Framework.Utils;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Testing
{
    public static class DotvvmTestHelper
    {
        public class FakeProtector : IViewModelProtector
        {
            // I hope I will not see this message anywhere on the web ;)
            public const string WarningPrefix = "WARNING - Message not encryped: ";
            public static readonly byte[] WarningPrefixBytes = Convert.FromBase64String("WARNING/NOT/ENCRYPTED+++");

            public string Protect(string serializedData, IDotvvmRequestContext context)
            {
                return WarningPrefix + ": " + serializedData;
            }

            public byte[] Protect(byte[] plaintextData, params string[] purposes)
            {
                var result = new List<byte>();
                result.AddRange(WarningPrefixBytes);
                result.AddRange(plaintextData);
                return result.ToArray();
            }

            public string Unprotect(string protectedData, IDotvvmRequestContext context)
            {
                if (!protectedData.StartsWith(WarningPrefix + ": ", StringComparison.Ordinal)) throw new SecurityException($"");
                return protectedData.Remove(0, WarningPrefix.Length + 2);
            }

            public byte[] Unprotect(byte[] protectedData, params string[] purposes)
            {
                if (!protectedData.Take(WarningPrefixBytes.Length).SequenceEqual(WarningPrefixBytes)) throw new SecurityException($"");
                return protectedData.Skip(WarningPrefixBytes.Length).ToArray();
            }
        }

        public class NopProtector : IViewModelProtector
        {
            public string Protect(string serializedData, IDotvvmRequestContext context) => "XXX";
            public byte[] Protect(byte[] plaintextData, params string[] purposes) => Convert.FromBase64String("XXXX");
            public string Unprotect(string protectedData, IDotvvmRequestContext context) => throw new NotImplementedException();
            public byte[] Unprotect(byte[] protectedData, params string[] purposes) => throw new NotImplementedException();
        }

        public class FakeCsrfProtector : ICsrfProtector
        {
            public string GenerateToken(IDotvvmRequestContext context)
            {
                return "Not a CSRF token.";
            }

            public void VerifyToken(IDotvvmRequestContext context, string token)
            {
                if (token != "Not a CSRF token.")
                    throw new Exception();
            }
        }

        public static void RegisterMoqServices(IServiceCollection services)
        {
            services.TryAddSingleton<IViewModelProtector, FakeProtector>();
            services.TryAddSingleton<ICsrfProtector, FakeCsrfProtector>();
            services.TryAddSingleton<IDotvvmCacheAdapter, SimpleDictionaryCacheAdapter>();
        }

        private static Lazy<DotvvmConfiguration> _defaultConfig = new Lazy<DotvvmConfiguration>(() => {
            var config = CreateConfiguration();
            config.RouteTable.Add("TestRoute", "TestRoute", "TestView.dothtml");
            config.Freeze();
            return config;
        });
        public static DotvvmConfiguration DefaultConfig => _defaultConfig.Value;

        public static DotvvmConfiguration CreateConfiguration(Action<IServiceCollection>? customServices = null) =>
            DotvvmConfiguration.CreateDefault(s => {
                customServices?.Invoke(s);
                RegisterMoqServices(s);
            });

        public static TestDotvvmRequestContext CreateContext(
            DotvvmConfiguration? configuration = null,
            RouteBase? route = null)
        {
            configuration = configuration ?? DefaultConfig;
            var httpContext = new TestHttpContext();
            IServiceProvider services = configuration.ServiceProvider.CreateScope().ServiceProvider;
            var context = new TestDotvvmRequestContext() {
                Configuration = configuration,
                Services = services,
                CsrfToken = "Test CSRF Token",
                ModelState = new ModelState(),
                ResourceManager = services.GetService<ResourceManager>(),
                HttpContext = httpContext,
                Parameters = new Dictionary<string, object>(),
                Route = route ?? configuration.RouteTable.FirstOrDefault(),
            };
            if (context.Route != null)
            {
                httpContext.Request.Path = context.TranslateVirtualPath(context.Route.BuildUrl(context.Parameters));
            }
            return context;
        }

        public static void CheckForErrors(DothtmlNode? node)
        {
            if (node is null) return;

            foreach (var n in node.EnumerateNodes())
                if (n.HasNodeErrors)
                    throw new DotvvmCompilationException(string.Join(", ", n.NodeErrors), n.Tokens);
        }
        public static ResolvedTreeRoot ParseResolvedTree(string markup, string fileName = "default.dothtml", DotvvmConfiguration? configuration = null, bool checkErrors = true)
        {
            configuration = configuration ?? DefaultConfig;

            var tokenizer = new DothtmlTokenizer();
            tokenizer.Tokenize(markup);

            var parser = new DothtmlParser();
            var tree = parser.Parse(tokenizer.Tokens);

            if (checkErrors) CheckForErrors(tree);

            var controlTreeResolver = configuration.ServiceProvider.GetRequiredService<IControlTreeResolver>();
            var controlResolver = configuration.ServiceProvider.GetRequiredService<IControlResolver>();
            var validator = ActivatorUtilities.CreateInstance<ControlUsageValidationVisitor>(configuration.ServiceProvider);

            var visitors = configuration.ServiceProvider.GetRequiredService<IOptions<ViewCompilerConfiguration>>().Value.TreeVisitors;
            var root = controlTreeResolver.ResolveTree(tree, fileName).CastTo<ResolvedTreeRoot>();
            if (checkErrors) CheckForErrors(root.DothtmlNode);
            foreach (var v in visitors)
            {
                v().VisitView(root);
            }
            if (checkErrors)
                validator.VisitAndAssert(root);
            else
                validator.VisitView(root);
            return root;
        }

        public static void EnsureCompiledAssemblyCache()
        {
            if (CompiledAssemblyCache.Instance == null)
            {
                new CompiledAssemblyCache(DefaultConfig);
            }
        }
    }
}
