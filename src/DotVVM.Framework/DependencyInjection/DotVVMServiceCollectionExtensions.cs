using System;
using System.Collections.Generic;
using System.Diagnostics;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Styles;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Diagnostics;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DotvvmServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential DotVVM services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        public static IServiceCollection RegisterDotVVMServices(IServiceCollection services)
        {
            services.AddOptions();

            services.AddDiagnosticServices();

            services.TryAddSingleton<IDotvvmViewBuilder, DefaultDotvvmViewBuilder>();
            services.TryAddSingleton<IViewModelSerializer, DefaultViewModelSerializer>();
            services.TryAddSingleton<IViewModelLoader, DefaultViewModelLoader>();
            services.TryAddSingleton<IViewModelValidationMetadataProvider, AttributeViewModelValidationMetadataProvider>();
            services.TryAddSingleton<IValidationRuleTranslator, ViewModelValidationRuleTranslator>();
            services.TryAddSingleton<IViewModelValidator, ViewModelValidator>();
            services.TryAddSingleton<IViewModelSerializationMapper, ViewModelSerializationMapper>();
            services.TryAddSingleton<IViewModelParameterBinder, AttributeViewModelParameterBinder>();
            services.TryAddSingleton<IOutputRenderer, DefaultOutputRenderer>();
            services.TryAddSingleton<IDotvvmPresenter, DotvvmPresenter>();
            services.TryAddSingleton<IMarkupFileLoader, AggregateMarkupFileLoader>();
            services.TryAddSingleton<IControlBuilderFactory, DefaultControlBuilderFactory>();
            services.TryAddSingleton<IControlResolver, DefaultControlResolver>();
            services.TryAddSingleton<IControlTreeResolver, DefaultControlTreeResolver>();
            services.TryAddSingleton<IAbstractTreeBuilder, ResolvedTreeBuilder>();
            services.TryAddSingleton<Func<ControlUsageValidationVisitor>>(s => () => ActivatorUtilities.CreateInstance<ControlUsageValidationVisitor>(s));
            services.TryAddSingleton<IViewCompiler, DefaultViewCompiler>();
            services.TryAddSingleton<IBindingCompiler, BindingCompiler>();
            services.TryAddSingleton<IBindingExpressionBuilder, BindingExpressionBuilder>();
            services.TryAddSingleton<BindingCompilationService, BindingCompilationService>();
            services.TryAddSingleton<DataPager.CommonBindings>();
            services.TryAddSingleton<IControlUsageValidator, DefaultControlUsageValidator>();
            services.TryAddSingleton<ILocalResourceUrlManager, LocalResourceUrlManager>();
            services.TryAddSingleton<IResourceHashService, DefaultResourceHashService>();
            services.TryAddSingleton<StaticCommandBindingCompiler, StaticCommandBindingCompiler>();
            services.TryAddSingleton<JavascriptTranslator, JavascriptTranslator>();
            services.TryAddSingleton<IHttpRedirectService, DefaultHttpRedirectService>();
            services.TryAddSingleton<IExpressionToDelegateCompiler, DefaultExpressionToDelegateCompiler>();


            services.TryAddScoped<AggregateRequestTracer, AggregateRequestTracer>();
            services.TryAddScoped<ResourceManager, ResourceManager>();
            services.TryAddSingleton(s => DotvvmConfiguration.CreateDefault(s));
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().Markup);
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().Resources);
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().RouteTable);
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().Runtime);
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().Security);
            services.TryAddSingleton(s => s.GetRequiredService<DotvvmConfiguration>().Styles);

            services.ConfigureWithServices<BindingCompilationOptions>((o, s) => {
                 o.TransformerClasses.Add(ActivatorUtilities.CreateInstance<BindingPropertyResolvers>(s));
            });

            services.ConfigureWithServices<ViewCompilerConfiguration>((o, s) => {
                var requiredResourceControl = s.GetRequiredService<IControlResolver>().ResolveControl(new ResolvedTypeDescriptor(typeof(RequiredResource)));
                o.TreeVisitors.Add(() => new BindingRequiredResourceVisitor((ControlResolverMetadata)requiredResourceControl));
                o.TreeVisitors.Add(() => ActivatorUtilities.CreateInstance<StylingVisitor>(s));
                o.TreeVisitors.Add(() => ActivatorUtilities.CreateInstance<DataContextPropertyAssigningVisitor>(s));
                o.TreeVisitors.Add(() => new LifecycleRequirementsAssigningVisitor());
            });

            return services;
        }

        internal static IServiceCollection AddDiagnosticServices(this IServiceCollection services)
        {
            services.TryAddSingleton<DotvvmDiagnosticsConfiguration>();
            services.TryAddSingleton<IDiagnosticsInformationSender, DiagnosticsInformationSender>();

            services.TryAddSingleton<IOutputRenderer, DiagnosticsRenderer>();
            services.AddScoped<DiagnosticsRequestTracer>(s => new DiagnosticsRequestTracer(s.GetRequiredService<IDiagnosticsInformationSender>()));
            services.AddScoped<IRequestTracer>(s => {
                var config = s.GetRequiredService<DotvvmConfiguration>();
                return (config.Debug ? (IRequestTracer)s.GetService<DiagnosticsRequestTracer>() : null) ?? new NullRequestTracer();
            });

            return services;
        }

        public static void ConfigureWithServices<TObject, TService>(this IServiceCollection services, Action<TObject, TService> configure)
            where TObject: class
        {
            services.AddSingleton<IConfigureOptions<TObject>>(s => new ConfigureOptions<TObject>(o => configure(o, s.GetRequiredService<TService>())));
        }

        public static void ConfigureWithServices<TObject, TService1, TService2>(this IServiceCollection services, Action<TObject, TService1, TService2> configure)
            where TObject: class
        {
            services.AddSingleton<IConfigureOptions<TObject>>(s => new ConfigureOptions<TObject>(o => configure(o, s.GetRequiredService<TService1>(), s.GetRequiredService<TService2>())));
        }

        public static void ConfigureWithServices<TObject>(this IServiceCollection services, Action<TObject, IServiceProvider> configure)
            where TObject: class
        {
            services.AddSingleton<IConfigureOptions<TObject>>(s => new ConfigureOptions<TObject>(o => configure(o, s)));
        }
    }
}
