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

namespace DotVVM.Framework.Controls.DynamicData
{
    public static class DynamicDataExtensions
    {

        /// <summary>
        /// Registers the Dynamic Data controls and return the Dynamic Data configuration.
        /// </summary>
        public static DynamicDataConfiguration AddDynamicDataConfiguration(this DotvvmConfiguration config, DynamicDataConfiguration dynamicDataConfiguration = null)
        {
            if (dynamicDataConfiguration == null)
            {
                dynamicDataConfiguration = new DynamicDataConfiguration();
            }

            config.Markup.AddCodeControl("dd", typeof(DynamicDataExtensions).Namespace, typeof(DynamicDataExtensions).Assembly.GetName().Name);

            var propertyDisplayMetadataProvider = new DataAnnotationsPropertyDisplayMetadataProvider();

            config.ServiceLocator.RegisterSingleton<IPropertyDisplayMetadataProvider>(() => propertyDisplayMetadataProvider);
            config.ServiceLocator.RegisterSingleton<IEntityPropertyListProvider>(() => new DefaultEntityPropertyListProvider(propertyDisplayMetadataProvider));
            config.ServiceLocator.RegisterSingleton<DynamicDataConfiguration>(() => dynamicDataConfiguration);
            config.ServiceLocator.RegisterSingleton<IFormBuilder>(() => dynamicDataConfiguration.FormBuilder);

            return dynamicDataConfiguration;
        }

        /// <summary>
        /// Registers the viewmodel metadata provider which uses resource files to get default error messages and property display names.
        /// </summary>
        public static void RegisterResourceMetadataProvider(this DotvvmConfiguration config, Type errorMessagesResourceFile, Type propertyDisplayNamesResourceFile)
        {
            var baseValidationMetadataProvider = config.ServiceLocator.GetService<IViewModelValidationMetadataProvider>();

            var basePropertyDisplayMetadataProvider = config.ServiceLocator.GetService<IPropertyDisplayMetadataProvider>();
            var newPropertyDisplayMetadataProvider = new ResourcePropertyDisplayMetadataProvider(propertyDisplayNamesResourceFile, basePropertyDisplayMetadataProvider);

            config.ServiceLocator.RegisterSingleton<IPropertyDisplayMetadataProvider>(() => newPropertyDisplayMetadataProvider);
            config.ServiceLocator.RegisterSingleton<IViewModelValidationMetadataProvider>(() => new ResourceViewModelValidationMetadataProvider(
                errorMessagesResourceFile, newPropertyDisplayMetadataProvider, baseValidationMetadataProvider)
            );
            config.ServiceLocator.RegisterSingleton<IEntityPropertyListProvider>(() => new DefaultEntityPropertyListProvider(newPropertyDisplayMetadataProvider));
        }

    }
}
