using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.DynamicData.Builders;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Controls.DynamicData.Metadata;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class DynamicDataExtensions
    {

        /// <summary>
        /// Registers the Dynamic Data controls and return the Dynamic Data configuration.
        /// </summary>
        public static DynamicDataConfiguration AddDynamicDataConfiguration(this IDotvvmOptions options, DynamicDataConfiguration dynamicDataConfiguration = null)
        {
            if (dynamicDataConfiguration == null)
            {
                dynamicDataConfiguration = new DynamicDataConfiguration();
            }

            var propertyDisplayMetadataProvider = new DataAnnotationsPropertyDisplayMetadataProvider();
            options.Services.AddSingleton<IPropertyDisplayMetadataProvider>(provider => propertyDisplayMetadataProvider);
            options.Services.AddSingleton<IEntityPropertyListProvider>(provider => new DefaultEntityPropertyListProvider(propertyDisplayMetadataProvider));
            options.Services.AddSingleton(provider => dynamicDataConfiguration);
            options.Services.AddSingleton(provider => dynamicDataConfiguration.FormBuilder);


            var provider2 = options.Services.BuildServiceProvider();
            var dotvvmConfiguration = provider2.GetService<DotvvmConfiguration>();
            dotvvmConfiguration.Markup.AddCodeControls(dynamicDataConfiguration.ControlsPrefix, typeof(DynamicDataExtensions).Namespace, typeof(DynamicDataExtensions).Assembly.GetName().Name);
            



            return dynamicDataConfiguration;
        }

        /// <summary>
        /// Registers the viewmodel metadata provider which uses resource files to get default error messages and property display names.
        /// </summary>
        public static void RegisterResourceMetadataProvider(this IDotvvmOptions options, Type errorMessagesResourceFile, Type propertyDisplayNamesResourceFile)
        {
            options.Services.AddSingleton<IPropertyDisplayMetadataProvider>(provider =>
            {
                var basePropertyDisplayMetadataProvider = provider.GetService<IPropertyDisplayMetadataProvider>();
                var newPropertyDisplayMetadataProvider = new ResourcePropertyDisplayMetadataProvider(propertyDisplayNamesResourceFile, basePropertyDisplayMetadataProvider);
                return newPropertyDisplayMetadataProvider;
            });
            options.Services.AddSingleton<IViewModelValidationMetadataProvider>(provider =>
            {
                var baseValidationMetadataProvider = provider.GetService<IViewModelValidationMetadataProvider>();
                var basePropertyDisplayMetadataProvider = provider.GetService<IPropertyDisplayMetadataProvider>();
                var newPropertyDisplayMetadataProvider = new ResourcePropertyDisplayMetadataProvider(propertyDisplayNamesResourceFile, basePropertyDisplayMetadataProvider);
                return new ResourceViewModelValidationMetadataProvider(errorMessagesResourceFile, newPropertyDisplayMetadataProvider, baseValidationMetadataProvider);
            });

            options.Services.AddSingleton<IEntityPropertyListProvider>(provider =>
            {

                var basePropertyDisplayMetadataProvider = provider.GetService<IPropertyDisplayMetadataProvider>();
                var newPropertyDisplayMetadataProvider = new ResourcePropertyDisplayMetadataProvider(propertyDisplayNamesResourceFile, basePropertyDisplayMetadataProvider);
                return new DefaultEntityPropertyListProvider(newPropertyDisplayMetadataProvider);
            });
        }

    }
}
