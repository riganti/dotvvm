using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls.DynamicData.Builders;
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
            var builder = dynamicDataContext.RequestContext.Configuration.ServiceLocator.GetService<IFormBuilder>();
            builder.BuildForm(this, dynamicDataContext);
        }

        private DynamicDataContext CreateDynamicDataContext(IDotvvmRequestContext context)
        {
            var dataContextStack = DataContextStackHelper.CreateDataContextStack(this);
            return new DynamicDataContext(dataContextStack, context)
            {
                ViewName = ViewName,
                GroupName = GroupName
            };
        }
    }
}