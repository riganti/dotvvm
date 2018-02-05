using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Hosting;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Repeats a template for each item in the DataSource collection.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(ItemTemplate))]
    public class Repeater : ItemsControl
    {
        private EmptyData emptyDataContainer;

        public Repeater(bool allowImplicitLifecycleRequirements = true)
        {
            SetValue(Internal.IsNamingContainerProperty, true);
            if (allowImplicitLifecycleRequirements && GetType() == typeof(Repeater))
            {
                LifecycleRequirements &= ~(ControlLifecycleRequirements.InvokeMissingInit | ControlLifecycleRequirements.InvokeMissingLoad);
            }
        }

        /// <summary>
        /// Gets or sets the template which will be displayed when the DataSource is empty.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate EmptyDataTemplate
        {
            get { return (ITemplate)GetValue(EmptyDataTemplateProperty); }
            set { SetValue(EmptyDataTemplateProperty, value); }
        }

        public static readonly DotvvmProperty EmptyDataTemplateProperty =
            DotvvmProperty.Register<ITemplate, Repeater>(t => t.EmptyDataTemplate, null);

        /// <summary>
        /// Gets or sets the template for each Repeater item.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement, Required = true)]
        [ControlPropertyBindingDataContextChange(nameof(DataSource))]
        [CollectionElementDataContextChange(1)]
        public ITemplate ItemTemplate
        {
            get { return (ITemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DotvvmProperty ItemTemplateProperty =
            DotvvmProperty.Register<ITemplate, Repeater>(t => t.ItemTemplate, null);

        /// <summary>
        /// Gets or sets whether the control should render a wrapper element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderWrapperTag
        {
            get { return (bool)GetValue(RenderWrapperTagProperty); }
            set { SetValue(RenderWrapperTagProperty, value); }
        }

        public static readonly DotvvmProperty RenderWrapperTagProperty =
            DotvvmProperty.Register<bool, Repeater>(t => t.RenderWrapperTag, true);

        /// <summary>
        /// Gets or sets the template containing the elements that separate items.
        /// </summary>
        [MarkupOptions(AllowBinding = false, MappingMode = MappingMode.InnerElement)]
        public ITemplate SeparatorTemplate
        {
            get { return (ITemplate)GetValue(SeparatorTemplateProperty); }
            set { SetValue(SeparatorTemplateProperty, value); }
        }

        public static readonly DotvvmProperty SeparatorTemplateProperty =
            DotvvmProperty.Register<ITemplate, Repeater>(t => t.SeparatorTemplate, null);

        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }

        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, Repeater>(t => t.WrapperTagName, "div");

        protected override bool RendersHtmlTag => RenderWrapperTag;

        /// <summary>
        /// Occurs after the viewmodel is applied to the page and before the commands are executed.
        /// </summary>
        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            SetChildren(context);
            base.OnLoad(context);
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            SetChildren(context);     // TODO: we should handle observable collection operations to persist controlstate of controls inside the Repeater
            base.OnPreRender(context);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            TagName = WrapperTagName;

            if (!RenderOnServer)
            {

                if (RenderWrapperTag)
                {
                    //writer.AddKnockoutForeachDataBind(javascriptDataSourceExpression);

                    writer.AddKnockoutDataBind("foreach", GetForeachKnockoutBindingGroup());
                }
                else
                {
                    writer.WriteKnockoutDataBindComment("foreach", GetForeachKnockoutBindingGroup().ToString());
                }
            }

            if (RenderWrapperTag)
            {
                base.RenderBeginTag(writer, context);
            }
        }

        private KnockoutBindingGroup GetForeachKnockoutBindingGroup()
        {
            var javascriptDataSourceExpression = GetForeachDataBindExpression().GetKnockoutBindingExpression(this);
            var group = new KnockoutBindingGroup();
            group.Add("data", javascriptDataSourceExpression);
            if (SeparatorTemplate != null)
            {
                group.Add("separatorTemplate", GetValueRaw(Internal.UniqueIDProperty) + "_separator", true);
            }
            return group;
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderOnServer)
            {
                // render on server
                foreach (var child in Children.Except(new[] { emptyDataContainer }))
                {
                    child.Render(writer, context);
                }
            }
            else
            {
                // render on client
                var itemContainer = GetItem(context);
                Children.Add(itemContainer);
                itemContainer.Render(writer, context);

            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderWrapperTag)
            {
                base.RenderEndTag(writer, context);
            }

            if (!RenderOnServer && !RenderWrapperTag)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            if (!RenderOnServer && SeparatorTemplate != null)
            {
                writer.AddAttribute("type", "text/html");
                writer.AddAttribute("id", GetValueRaw(Internal.UniqueIDProperty) + "_separator");
                var unique = GetValueRaw(Internal.UniqueIDProperty);
                var id = GetValueRaw(Internal.ClientIDFragmentProperty);
                writer.RenderBeginTag("script");
                GetSeparator(context).Render(writer, context);
                writer.RenderEndTag();
            }

            emptyDataContainer?.Render(writer, context);
        }

        private DotvvmControl GetEmptyItem(IDotvvmRequestContext context)
        {
            if (emptyDataContainer == null)
            {
                var dataSourceBinding = GetDataSourceBinding();
                emptyDataContainer = new EmptyData();
                emptyDataContainer.SetValue(EmptyData.RenderWrapperTagProperty, GetValueRaw(RenderWrapperTagProperty));
                emptyDataContainer.SetValue(EmptyData.WrapperTagNameProperty, GetValueRaw(WrapperTagNameProperty));
                emptyDataContainer.SetValue(EmptyData.VisibleProperty, GetValueRaw(VisibleProperty));
                emptyDataContainer.SetBinding(DataSourceProperty, dataSourceBinding);
                EmptyDataTemplate.BuildContent(context, emptyDataContainer);
            }
            return emptyDataContainer;
        }

        private DotvvmControl GetItem(IDotvvmRequestContext context, object item = null, int index = -1, IValueBinding itemBinding = null)
        {
            var container = new DataItemContainer();
            if (item == null && index == -1)
            {
                SetUpClientItem(container);
            }
            else
            {
                SetUpServerItem(context, item, index, itemBinding, container);
            }

            ItemTemplate.BuildContent(context, container);
            return container;
        }

        private DotvvmControl GetSeparator(IDotvvmRequestContext context)
        {
            var placeholder = new PlaceHolder();
            SeparatorTemplate.BuildContent(context, placeholder);
            return placeholder;
        }

        /// <summary>
        /// Performs the data-binding and builds the controls inside the <see cref="Repeater"/>.
        /// </summary>
        private void SetChildren(IDotvvmRequestContext context)
        {
            Children.Clear();
            emptyDataContainer = null;

            if (DataSource != null)
            {
                var itemBinding = GetItemBinding();
                var index = 0;
                foreach (var item in GetIEnumerableFromDataSource())
                {
                    if (SeparatorTemplate != null && index > 0)
                    {
                        Children.Add(GetSeparator(context));
                    }
                    Children.Add(GetItem(context, item, index, itemBinding));
                    index++;
                }
            }

            if (EmptyDataTemplate != null)
            {
                Children.Add(GetEmptyItem(context));
            }
        }

        private void SetUpClientItem(DataItemContainer container)
        {
            container.DataContext = null;
            container.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[$index]");
            container.SetValue(Internal.ClientIDFragmentProperty, GetValueRaw(Internal.CurrentIndexBindingProperty));
        }

        private void SetUpServerItem(IDotvvmRequestContext context, object item, int index, IValueBinding itemBinding, DataItemContainer container)
        {
            container.DataItemIndex = index;
            container.SetDataContextForItem(itemBinding, index, item);
            container.SetValue(Internal.PathFragmentProperty, GetPathFragmentExpression() + "/[" + index + "]");
            container.ID = index.ToString();
        }
    }
}
