using System;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders different content when the application is running in a specified environment and when it's not.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(IsEnvironmentTemplate))]
    public class EnvironmentView : ConfigurableHtmlControl
    {
        public EnvironmentView()
            : base("div")
        {
            RenderWrapperTag = false;
        }

        /// <summary>
        /// Gets or sets a comma-separated list of hosting environments (e.g. Development, Production).
        /// </summary>
        [MarkupOptions(AllowBinding = false, Required = true)]
        public string[] Environments
        {
            get { return (string[])GetValue(EnvironmentsProperty); }
            set { SetValue(EnvironmentsProperty, value); }
        }

        public static readonly DotvvmProperty EnvironmentsProperty
            = DotvvmProperty.Register<string[], EnvironmentView>(c => c.Environments);

        /// <summary>
        /// Gets or sets the content rendered when the application is running in one of the specified environments.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate IsEnvironmentTemplate
        {
            get { return (ITemplate)GetValue(IsEnvironmentTemplateProperty); }
            set { SetValue(IsEnvironmentTemplateProperty, value); }
        }

        public static readonly DotvvmProperty IsEnvironmentTemplateProperty
            = DotvvmProperty.Register<ITemplate, EnvironmentView>(c => c.IsEnvironmentTemplate);

        /// <summary>
        /// Gets or sets the content rendered when the application is not running in any of the specified environments.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, AllowBinding = false)]
        public ITemplate IsNotEnvironmentTemplate
        {
            get { return (ITemplate)GetValue(IsNotEnvironmentTemplateProperty); }
            set { SetValue(IsNotEnvironmentTemplateProperty, value); }
        }

        public static readonly DotvvmProperty IsNotEnvironmentTemplateProperty
            = DotvvmProperty.Register<ITemplate, EnvironmentView>(c => c.IsNotEnvironmentTemplate);

        protected internal override void OnInit(IDotvvmRequestContext context)
        {
            if (IsRunningInEnvironment(context))
            {
                IsEnvironmentTemplate?.BuildContent(context, this);
            }
            else
            {
                IsNotEnvironmentTemplate?.BuildContent(context, this);
            }
        }

        private bool IsRunningInEnvironment(IDotvvmRequestContext context)
        {
            var provider = context.Services.GetService<IEnvironmentNameProvider>();
            var currentEnvironmentName = provider?.GetEnvironmentName(context);

            if (Environments != null && currentEnvironmentName != null)
            {
                return Environments.Any(n => EnvironmentsEqual(n, currentEnvironmentName));
            }

            return false;
        }

        private bool EnvironmentsEqual(string environmentName, string currentEnvironmentName)
            => string.Equals(environmentName.Trim(), currentEnvironmentName, StringComparison.OrdinalIgnoreCase);
    }
}
