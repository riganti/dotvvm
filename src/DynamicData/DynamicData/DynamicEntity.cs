using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Builders;
using DotVVM.Framework.Controls.DynamicData.Configuration;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls.DynamicData
{
    public class DynamicEntity : HtmlGenericControl
    {
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


        /// <summary>
        /// Gets or sets whether the controls in the form are enabled or disabled.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty
            = DotvvmProperty.Register<bool, DynamicEntity>(c => c.Enabled, true, isValueInherited: true);





        internal static readonly DotvvmProperty DynamicDataContextProperty
            = DotvvmProperty.Register<DynamicDataContext, DynamicEntity>("DynamicDataContext", null);


        public DynamicEntity() : base("div")
        {
        }

        protected override void OnInit(IDotvvmRequestContext context)
        {
            var dynamicDataContext = CreateDynamicDataContext(context);
            SetValue(DynamicDataContextProperty, dynamicDataContext);

            if (ContentTemplate != null)
            {
                ContentTemplate.BuildContent(context, this);
            }
            else
            {
                BuildForm(dynamicDataContext);
            }

            base.OnInit(context);
        }

        protected virtual void BuildForm(DynamicDataContext dynamicDataContext)
        {
            var builder = dynamicDataContext.DynamicDataConfiguration.GetFormBuilder(FormBuilderName);
            builder.BuildForm(this, dynamicDataContext);
        }

        private DynamicDataContext CreateDynamicDataContext(IDotvvmRequestContext context)
        {
            return new DynamicDataContext(this.GetDataContextType(), context)
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }
    }
}