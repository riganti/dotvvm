﻿using System;
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
        /// <summary>
        /// Registers all services required by DotVVM Dynamic Data.
        /// </summary>
        public static IDotvvmServiceCollection AddDynamicData(this IDotvvmServiceCollection services, DynamicDataConfiguration dynamicDataConfiguration = null)
        {
            if (dynamicDataConfiguration == null)
            {
                dynamicDataConfiguration = new DynamicDataConfiguration();
            }
            
            // add the configuration of Dynamic Data to the service collection
            services.Services.AddSingleton(serviceProvider => dynamicDataConfiguration);

            RegisterDefaultProviders(services.Services, dynamicDataConfiguration);
            if (dynamicDataConfiguration.UseLocalizationResourceFiles)
            {
                RegisterResourceFileProviders(services.Services, dynamicDataConfiguration);
            }

            return services;
        }

        private static void RegisterDefaultProviders(IServiceCollection services, DynamicDataConfiguration dynamicDataConfiguration)
        {
            services.AddSingleton<IPropertyDisplayMetadataProvider>(
                serviceProvider => new DataAnnotationsPropertyDisplayMetadataProvider()
            );

            services.AddSingleton<IEntityPropertyListProvider>(
                serviceProvider => new DefaultEntityPropertyListProvider(serviceProvider.GetService<IPropertyDisplayMetadataProvider>())
            );
        }

        private static void RegisterResourceFileProviders(IServiceCollection services, DynamicDataConfiguration dynamicDataConfiguration)
        {
            if (dynamicDataConfiguration.PropertyDisplayNamesResourceFile == null)
            {
                throw new ArgumentException($"The {nameof(DynamicDataConfiguration)} must specify the {nameof(DynamicDataConfiguration.PropertyDisplayNamesResourceFile)} resource class!");
            }
            if (dynamicDataConfiguration.ErrorMessagesResourceFile == null)
            {
                throw new ArgumentException($"The {nameof(DynamicDataConfiguration)} must specify the {nameof(DynamicDataConfiguration.ErrorMessagesResourceFile)} resource class!");
            }

            services.Decorate<IPropertyDisplayMetadataProvider>(
                baseService => new ResourcePropertyDisplayMetadataProvider(
                    dynamicDataConfiguration.PropertyDisplayNamesResourceFile, baseService)
            );

            services.Decorate<IViewModelValidationMetadataProvider>(
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
            config.Markup.AddCodeControls("dd", typeof(DynamicDataExtensions).Namespace, typeof(DynamicDataExtensions).GetTypeInfo().Assembly.GetName().Name);
            return config.ServiceProvider.GetService<DynamicDataConfiguration>();
        }
        
    }
}
