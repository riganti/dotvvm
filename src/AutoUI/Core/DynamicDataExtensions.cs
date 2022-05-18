using System;
using System.Linq;
using DotVVM.AutoUI.Configuration;
using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.AutoUI
{
    public static class DynamicDataExtensions
    {
        /// <summary>
        /// Registers all services required by DotVVM Dynamic Data.
        /// </summary>
        public static IDotvvmServiceCollection AddAutoUI(this IDotvvmServiceCollection services, Action<DynamicDataConfiguration>? configure = null)
        {
            var dynamicDataConfiguration = new DynamicDataConfiguration();
            configure?.Invoke(dynamicDataConfiguration);
            
            // add the configuration of Dynamic Data to the service collection
            services.Services.AddSingleton(serviceProvider => dynamicDataConfiguration);

            RegisterDefaultProviders(services.Services, dynamicDataConfiguration);
            RegisterResourceFileProviders(services.Services, dynamicDataConfiguration);

            services.Services.Configure<DotvvmConfiguration>(AddDynamicDataConfiguration);

            return services;
        }

        private static void RegisterDefaultProviders(IServiceCollection services, DynamicDataConfiguration dynamicDataConfiguration)
        {
            services.AddSingleton<IPropertyDisplayMetadataProvider>(
                serviceProvider => new DataAnnotationsPropertyDisplayMetadataProvider(dynamicDataConfiguration)
            );

            services.AddSingleton<IEntityPropertyListProvider>(
                serviceProvider => new DefaultEntityPropertyListProvider(serviceProvider.GetService<IPropertyDisplayMetadataProvider>())
            );
        }

        private static void RegisterResourceFileProviders(IServiceCollection services, DynamicDataConfiguration dynamicDataConfiguration)
        {
            if (dynamicDataConfiguration.PropertyDisplayNamesResourceFile != null)
            {
                services.Decorate<IPropertyDisplayMetadataProvider>(
                    baseService => new ResourcePropertyDisplayMetadataProvider(
                        dynamicDataConfiguration.PropertyDisplayNamesResourceFile, baseService)
                );
            }

            if (dynamicDataConfiguration.ErrorMessagesResourceFile != null)
            {
                services.Decorate<IViewModelValidationMetadataProvider>(
                    (baseService, serviceProvider) => new ResourceViewModelValidationMetadataProvider(
                        dynamicDataConfiguration.ErrorMessagesResourceFile,
                        serviceProvider.GetService<IPropertyDisplayMetadataProvider>(),
                        baseService)
                );
            }
        }


        /// <summary>
        /// Registers the Dynamic Data controls and return the Dynamic Data configuration.
        /// </summary>
        private static void AddDynamicDataConfiguration(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("au", typeof(DynamicDataExtensions).Namespace! + ".Controls", typeof(DynamicDataExtensions).Assembly!.GetName().Name!);

            RegisterDynamicDataStyles(config);
        }


        private static void RegisterDynamicDataStyles(DotvvmConfiguration config)
        {
            var s = config.Styles;
            s.Register<DynamicColumns>()
                .SetDotvvmProperty(Styles.AppendProperty, c => DynamicColumns.Replace(c))
                .ReplaceWith(new DummyColumnThatDoesNothing());
            s.Register<DynamicGridColumn>()
                .ReplaceWith(c => DynamicGridColumn.Replace(c));


            // bulma styles
            s.Register<CheckBox>(c => c.PropertyValue<string[]>(DynamicEditor.TagsProperty).Contains("bulma"))
                .AddClass("checkbox");

            s.Register<SelectorBase>(c => c.PropertyValue<string[]>(DynamicEditor.TagsProperty).Contains("bulma"))
                .SetAttribute("class", "")
                .WrapWith(new HtmlGenericControl("div").AddCssClass("select"));

        }
        
    }
}
