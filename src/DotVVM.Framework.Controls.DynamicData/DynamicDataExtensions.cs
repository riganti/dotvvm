using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class DynamicDataExtensions
    {


        public static IDotvvmBuilder ConfigureDynamicData(this IDotvvmBuilder builder, DynamicDataConfiguration dynamicDataConfiguration = null)
        {
            if (dynamicDataConfiguration == null)
            {
                dynamicDataConfiguration = new DynamicDataConfiguration();
            }

            // add the configuration of Dynamic Data to the service collection
            builder.Services.AddSingleton(serviceProvider => dynamicDataConfiguration);

            RegisterDefaultProviders(builder, dynamicDataConfiguration);
            if (dynamicDataConfiguration.UseLocalizationResourceFiles)
            {
                RegisterResourceFileProviders(builder, dynamicDataConfiguration);
            }

            return builder;
        }

        private static void RegisterDefaultProviders(IDotvvmBuilder builder, DynamicDataConfiguration dynamicDataConfiguration)
        {
            builder.Services.AddSingleton<IPropertyDisplayMetadataProvider>(
                serviceProvider => new DataAnnotationsPropertyDisplayMetadataProvider()
            );

            builder.Services.AddSingleton<IEntityPropertyListProvider>(
                serviceProvider => new DefaultEntityPropertyListProvider(serviceProvider.GetService<IPropertyDisplayMetadataProvider>())
            );
        }

        private static void RegisterResourceFileProviders(IDotvvmBuilder builder, DynamicDataConfiguration dynamicDataConfiguration)
        {
            if (dynamicDataConfiguration.PropertyDisplayNamesResourceFile == null)
            {
                throw new ArgumentException($"The {nameof(DynamicDataConfiguration)} must specify the {nameof(DynamicDataConfiguration.PropertyDisplayNamesResourceFile)} resource class!");
            }
            if (dynamicDataConfiguration.ErrorMessagesResourceFile == null)
            {
                throw new ArgumentException($"The {nameof(DynamicDataConfiguration)} must specify the {nameof(DynamicDataConfiguration.ErrorMessagesResourceFile)} resource class!");
            }

            builder.Services.Decorate<IPropertyDisplayMetadataProvider>(
                baseService => new ResourcePropertyDisplayMetadataProvider(
                    dynamicDataConfiguration.PropertyDisplayNamesResourceFile, baseService)
            );

            builder.Services.Decorate<IViewModelValidationMetadataProvider>(
                (baseService, serviceProvider) => new ResourceViewModelValidationMetadataProvider(
                    dynamicDataConfiguration.ErrorMessagesResourceFile, 
                    serviceProvider.GetService<IPropertyDisplayMetadataProvider>(), 
                    baseService)
            );
        }


        /// <summary>
        /// Registers the Dynamic Data controls and return the Dynamic Data configuration.
        /// </summary>
        public static DynamicDataConfiguration AddDynamicDataConfiguration(this DotvvmConfiguration config)
        {
            config.Markup.AddCodeControl("dd", typeof(DynamicDataExtensions).Namespace, typeof(DynamicDataExtensions).GetTypeInfo().Assembly.GetName().Name);
            return config.ServiceLocator.GetService<DynamicDataConfiguration>();
        }
        
    }
}
