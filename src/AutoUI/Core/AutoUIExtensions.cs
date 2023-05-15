using System;
using System.Linq;
using DotVVM.AutoUI.Configuration;
using DotVVM.AutoUI.Controls;
using DotVVM.AutoUI.Metadata;
using DotVVM.AutoUI.PropertyHandlers;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotVVM.AutoUI
{
    public static class AutoUIExtensions
    {
        /// <summary>
        /// Registers all services required by DotVVM AutoUI.
        /// </summary>
        public static IDotvvmServiceCollection AddAutoUI(this IDotvvmServiceCollection services, Action<AutoUIConfiguration>? configure = null)
        {
            var autoUiConfiguration = new AutoUIConfiguration();
            configure?.Invoke(autoUiConfiguration);
            
            // add the configuration of AutoUI to the service collection
            services.Services.AddSingleton(serviceProvider =>
            {
                foreach (var conf in serviceProvider.GetServices<IConfigureOptions<AutoUIConfiguration>>())
                {
                    conf.Configure(autoUiConfiguration);
                }
                return autoUiConfiguration;
            });
            
            RegisterDefaultProviders(services.Services, autoUiConfiguration);
            RegisterResourceFileProviders(services.Services, autoUiConfiguration);

            services.Services.Configure<DotvvmConfiguration>(AddAutoUIConfiguration);
            services.Services.AddSingleton<ISelectorDiscoveryService, SelectorDiscoveryService>();

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
                        baseService)
                );
            }
        }


        /// <summary> Registers the AutoUI controls </summary>
        private static void AddAutoUIConfiguration(DotvvmConfiguration config)
        {
            config.Markup.AddAssembly(typeof(Annotations.Selection).Assembly);
            config.Markup.AddCodeControls("auto", exampleControl: typeof(AutoForm));

            RegisterAutoUIStyles(config);
        }


        private static void RegisterAutoUIStyles(DotvvmConfiguration config)
        {
            var s = config.Styles;
            s.Register<AutoGridViewColumns>()
                .SetDotvvmProperty(Styles.AppendProperty, c => AutoGridViewColumns.Replace(c))
                .Remove();
            s.Register<AutoGridViewColumn>()
                .ReplaceWith(c => AutoGridViewColumn.Replace(c));

            // bootstrap styles
            s.Register<BootstrapForm>()
                .SetDotvvmProperty(Validator.InvalidCssClassProperty, "is-invalid");
            s.Register<ComboBox>(c => c.HasAncestor<BootstrapForm>())
                .AddClass(c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormSelectCssClass)!);
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BootstrapForm>()
                                                && c.PropertyValue<bool>(BootstrapForm.RequiresFormSelectCssClassProperty))
                .AddClass(c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormSelectCssClass)!);
            s.Register<TextBox>(c => c.HasAncestor<BootstrapForm>())
                .AddClass(c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormControlCssClass)!);
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BootstrapForm>()
                                                && c.PropertyValue<bool>(BootstrapForm.RequiresFormControlCssClassProperty))
                .AddClass(c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormControlCssClass)!);

            s.Register<CheckBox>(c => c.HasAncestor<BootstrapForm>())
                .SetProperty(c => c.LabelCssClass, c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormCheckLabelCssClass)!)
                .SetProperty(c => c.InputCssClass, c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormCheckInputCssClass)!);
            s.Register<RadioButton>(c => c.HasAncestor<BootstrapForm>())
                .SetProperty(c => c.LabelCssClass, c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormCheckLabelCssClass)!)
                .SetProperty(c => c.InputCssClass, c => c.AncestorsOfType<BootstrapForm>().First().Property(p => p.FormCheckInputCssClass)!);

            // bulma styles
            s.Register<CheckBox>(c => c.HasAncestor<BulmaForm>())
                .AddClass("checkbox");
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BulmaForm>()
                                      && c.PropertyValue<bool>(BulmaForm.WrapWithCheckboxClassProperty))
                .AddClass("checkbox");

            s.Register<RadioButton>(c => c.HasAncestor<BulmaForm>())
                .AddClass("radio");
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BulmaForm>()
                                                && c.PropertyValue<bool>(BulmaForm.WrapWithRadioClassProperty))
                .AddClass("radio");

            s.Register<TextBox>(c => c.HasAncestor<BulmaForm>()
                                     && c.PropertyValue<TextBoxType>(TextBox.TypeProperty) == TextBoxType.MultiLine)
                .AddClass("textarea");
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BulmaForm>()
                                                && c.PropertyValue<bool>(BulmaForm.WrapWithTextareaClassProperty))
                .AddClass("textarea");

            s.Register<TextBox>(c => c.HasAncestor<BulmaForm>()
                                     && c.PropertyValue<TextBoxType>(TextBox.TypeProperty) != TextBoxType.MultiLine)
                .AddClass("input");
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BulmaForm>()
                                                && c.PropertyValue<bool>(BulmaForm.WrapWithInputClassProperty))
                .AddClass("input");

            s.Register<ComboBox>(c => c.HasAncestor<BulmaForm>())
                .WrapWith(new HtmlGenericControl("div").AddCssClass("select"));
            s.Register<HtmlGenericControl>(c => c.HasAncestor<BulmaForm>()
                                                && c.PropertyValue<bool>(BulmaForm.WrapWithSelectClassProperty))
                .WrapWith(new HtmlGenericControl("div").AddCssClass("select"));
        }
        
    }
}
