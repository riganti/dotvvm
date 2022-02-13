using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Builders;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.DynamicData
{
    [ControlMarkupOptions(Precompile = ControlPrecompilationMode.Always)]
    public class DynamicEntity : CompositeControl
    {
        private readonly IServiceProvider services;
        public DynamicEntity(IServiceProvider services)
        {
            this.services = services;
        }

        /// <summary>
        /// Gets or sets the custom layout of the form.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate ContentTemplate
        {
            get { return (ITemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ContentTemplateProperty
            = DotvvmProperty.Register<ITemplate, DynamicEntity>(c => c.ContentTemplate, null);


        /// <summary>
        /// Gets or sets the view name (e.g. Insert, Edit, ReadOnly). Some fields may have different metadata for each view.
        /// </summary>
        public string ViewName
        {
            get { return (string)GetValue(ViewNameProperty); }
            set { SetValue(ViewNameProperty, value); }
        }
        public static readonly DotvvmProperty ViewNameProperty
            = DotvvmProperty.Register<string, DynamicEntity>(c => c.ViewName, null);


        /// <summary>
        /// Gets or sets the group of fields that should be rendered. If not set, fields from all groups will be rendered.
        /// </summary>
        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public static readonly DotvvmProperty GroupNameProperty
            = DotvvmProperty.Register<string, DynamicEntity>(c => c.GroupName, null);


        /// <summary>
        /// Gets or sets the name of the form builder to be used. If not set, the default form builder is used.
        /// </summary>
        public string FormBuilderName
        {
            get { return (string)GetValue(FormBuilderNameProperty); }
            set { SetValue(FormBuilderNameProperty, value); }
        }
        public static readonly DotvvmProperty FormBuilderNameProperty
            = DotvvmProperty.Register<string, DynamicEntity>(c => c.FormBuilderName, "");


        public DotvvmControl GetContents()
        {

            if (ContentTemplate != null)
            {
                return new TemplateHost(ContentTemplate);
            }
            else
            {
                var dynamicDataContext = CreateDynamicDataContext();
                return BuildForm(dynamicDataContext);
            }
        }

        protected virtual DotvvmControl BuildForm(DynamicDataContext dynamicDataContext)
        {
            var builder = dynamicDataContext.DynamicDataConfiguration.GetFormBuilder(FormBuilderName);
            return builder.BuildForm(dynamicDataContext);
        }

        private DynamicDataContext CreateDynamicDataContext()
        {
            return new DynamicDataContext(this.GetDataContextType(), this.services)
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }
    }
}
