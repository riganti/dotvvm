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
    public static class AutoUIExtensions
    {
        /// <summary>
        /// Registers all services required by DotVVM Dynamic Data.
        /// </summary>
        public static IDotvvmServiceCollection AddAutoUI(this IDotvvmServiceCollection services, Action<DynamicDataConfiguration>? configure = null)
        {
            var autoUiConfiguration = new DynamicDataConfiguration();
            configure?.Invoke(autoUiConfiguration);
            
            // add the configuration of Dynamic Data to the service collection
            services.Services.AddSingleton(serviceProvider => autoUiConfiguration);

            RegisterDefaultProviders(services.Services, autoUiConfiguration);
            RegisterResourceFileProviders(services.Services, autoUiConfiguration);

            services.Services.Configure<DotvvmConfiguration>(AddAutoUIConfiguration);

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
        private static void AddAutoUIConfiguration(DotvvmConfiguration config)
        {
            config.Markup.AddCodeControls("auto", typeof(AutoUIExtensions).Namespace! + ".Controls", typeof(AutoUIExtensions).Assembly.FullName!);

            RegisterDynamicDataStyles(config);
        }


        private static void RegisterDynamicDataStyles(DotvvmConfiguration config)
        {
            var s = config.Styles;
            s.Register<AutoGridViewColumns>()
                .SetDotvvmProperty(Styles.AppendProperty, c => AutoGridViewColumns.Replace(c))
                .ReplaceWith(new DummyColumnThatDoesNothing());
            s.Register<AutoGridViewColumn>()
                .ReplaceWith(c => AutoGridViewColumn.Replace(c));

            // bulma styles
            s.Register<CheckBox>(c => c.PropertyValue<string[]>(AutoEditor.TagsProperty)!.Contains("bulma"))
                .AddClass("checkbox");

            s.Register<SelectorBase>(c => c.PropertyValue<string[]>(AutoEditor.TagsProperty)!.Contains("bulma"))
                .SetAttribute("class", "")
                .WrapWith(new HtmlGenericControl("div").AddCssClass("select"));

        }
        
    }
}
