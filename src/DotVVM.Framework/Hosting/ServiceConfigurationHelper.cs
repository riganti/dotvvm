using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public static class ServiceConfigurationHelper
    {
        public static void AddDotvvmCoreServices(IServiceCollection services)
        {
            //services.AddSingleton<ICsrfProtector, DefaultCsrfProtector>();
            //services.AddSingleton<IViewModelProtector, DefaultViewModelProtector>();
            services.AddSingleton<IDotvvmViewBuilder, DefaultDotvvmViewBuilder>();
            services.AddSingleton<IViewModelSerializer, DefaultViewModelSerializer>();
            services.AddSingleton<IViewModelLoader, DefaultViewModelLoader>();
            services.AddSingleton<IViewModelValidationMetadataProvider, AttributeViewModelValidationMetadataProvider>();
            services.AddSingleton<IValidationRuleTranslator, ViewModelValidationRuleTranslator>();
            services.AddSingleton<IViewModelValidator, ViewModelValidator>();
            services.AddSingleton<IViewModelSerializationMapper, ViewModelSerializationMapper>();
            services.AddSingleton<IOutputRenderer, DefaultOutputRenderer>();
            services.AddSingleton<IDotvvmPresenter, DotvvmPresenter>();
            services.AddSingleton<IMarkupFileLoader, DefaultMarkupFileLoader>();
            services.AddSingleton<IControlBuilderFactory, DefaultControlBuilderFactory>();
            services.AddSingleton<IControlResolver, DefaultControlResolver>();
            services.AddSingleton<IControlTreeResolver, DefaultControlTreeResolver>();
            services.AddSingleton<IAbstractTreeBuilder, ResolvedTreeBuilder>();
            services.AddSingleton<IViewCompiler, DefaultViewCompiler>();
            services.AddSingleton<IBindingCompiler, BindingCompiler>();
            services.AddSingleton<IBindingExpressionBuilder, BindingExpressionBuilder>();
            services.AddSingleton<IBindingIdGenerator, OriginalStringBindingIdGenerator>();
            services.AddSingleton<IControlUsageValidator, DefaultControlUsageValidator>();
            services.AddSingleton<DotvvmConfiguration>(s => DotvvmConfiguration.CreateDefault(s));
        }

        class DotvvmConfigurationContainer
        {
            public DotvvmConfiguration Configuration { get; set; }
        }
    }
}
