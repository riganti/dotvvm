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
        public static IDotvvmServiceCollection AddAutoUI(this IDotvvmServiceCollection services, Action<AutoUIConfiguration>? configure = null)
        {
            var autoUiConfiguration = new AutoUIConfiguration();
            configure?.Invoke(autoUiConfiguration);
            
            // add the configuration of Dynamic Data to the service collection
            services.Services.AddSingleton(serviceProvider => autoUiConfiguration);

            RegisterDefaultProviders(services.Services, autoUiConfiguration);
            RegisterResourceFileProviders(services.Services, autoUiConfiguration);

            services.Services.Configure<DotvvmConfiguration>(AddAutoUIConfiguration);

            return services;
        }

        private static void RegisterDefaultProviders(IServiceCollection services, AutoUIConfiguration autoUiConfiguration)
        {
            services.AddSingleton<IPropertyDisplayMetadataProvider>(
                serviceProvider => new DataAnnotationsPropertyDisplayMetadataProvider(autoUiConfiguration)
            );

            services.AddSingleton<IEntityPropertyListProvider>(
                serviceProvider => new DefaultEntityPropertyListProvider(serviceProvider.GetService<IPropertyDisplayMetadataProvider>())
            );
        }

        private static void RegisterResourceFileProviders(IServiceCollection services, AutoUIConfiguration autoUiConfiguration)
        {
            if (autoUiConfiguration.PropertyDisplayNamesResourceFile != null)
            {
                services.Decorate<IPropertyDisplayMetadataProvider>(
                    baseService => new ResourcePropertyDisplayMetadataProvider(
                        autoUiConfiguration.PropertyDisplayNamesResourceFile, baseService)
                );
            }

            if (autoUiConfiguration.ErrorMessagesResourceFile != null)
            {
                services.Decorate<IViewModelValidationMetadataProvider>(
                    (baseService, serviceProvider) => new ResourceViewModelValidationMetadataProvider(
                        autoUiConfiguration.ErrorMessagesResourceFile,
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

            RegisterAutoUIStyles(config);
        }


        private static void RegisterAutoUIStyles(DotvvmConfiguration config)
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
