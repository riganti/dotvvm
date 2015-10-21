using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System.Collections;
using System.Diagnostics;
using DotVVM.Framework.Runtime.Compilation.JavascriptCompilation;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Repeats a template for each item in the DataSource collection.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = nameof(ItemTemplate))]
    public class Repeater : ItemsControl
    {

        /// <summary>
        /// Gets or sets the template for each Repeater item.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement, Required = true)]
        [ControlPropertyBindingDataContextChange("DataSource")]
        [CollectionElementDataContextChange(1)]
        public ITemplate ItemTemplate
        {
            get { return (ITemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DotvvmProperty ItemTemplateProperty =
            DotvvmProperty.Register<ITemplate, Repeater>(t => t.ItemTemplate, null);

        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly DotvvmProperty WrapperTagNameProperty =
            DotvvmProperty.Register<string, Repeater>(t => t.WrapperTagName, "div");

        /// <summary>
        /// Gets or sets whether the control should render a wrapper element.
        /// </summary>
        public bool RenderWrapperTag
        {
            get { return (bool) GetValue(RenderWrapperTagProperty); }
            set { SetValue(RenderWrapperTagProperty, value); }
        }
        public static readonly DotvvmProperty RenderWrapperTagProperty =
            DotvvmProperty.Register<bool, Repeater>(t => t.RenderWrapperTag, true);


        public Repeater()
        {
            SetValue(Internal.IsNamingContainerProperty, true);
        }

        /// <summary>
        /// Occurs after the viewmodel is applied to the page and before the commands are executed.
        /// </summary>
        protected internal override void OnLoad(IDotvvmRequestContext context)
        {
            EnsureControlHasId();

            DataBind(context);
            base.OnLoad(context);
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal override void OnPreRender(IDotvvmRequestContext context)
        {
            DataBind(context);     // TODO: we should handle observable collection operations to persist controlstate of controls inside the Repeater
            base.OnPreRender(context);
        }

        private object[] lastBoundArray = null;

        /// <summary>
        /// Performs the data-binding and builds the controls inside the <see cref="Repeater"/>.
        /// </summary>
        private void DataBind(IDotvvmRequestContext context)
        {
            var dataSourceBinding = GetDataSourceBinding();

            var index = 0;
            var dataSource = DataSource;
            if (dataSource != null)
            {
                var items = GetIEnumerableFromDataSource(dataSource).Cast<object>().ToArray();
                if (lastBoundArray != null)
                {
                    if (lastBoundArray.SequenceEqual(items)) return;
                }
                Children.Clear();

                var javascriptDataSourceExpression = dataSourceBinding.GetKnockoutBindingExpression();
                foreach (var item in items)
                {
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    ItemTemplate.BuildContent(context, placeholder);
                    Children.Add(placeholder);
                    placeholder.SetBinding(DataContextProperty, GetItemBinding((IList)items, javascriptDataSourceExpression, index));
                    placeholder.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), index));
                    placeholder.ID = "i" + index;
                    Debug.Assert(placeholder.properties[DataContextProperty] != null);
                    index++;
                }
            }
        }
         

        protected override bool RendersHtmlTag => RenderWrapperTag;


        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            TagName = WrapperTagName;

            if (!RenderOnServer)
            {
                var javascriptDataSourceExpression = GetForeachDataBindJavascriptExpression();

                if (RenderWrapperTag)
                {
                    writer.AddKnockoutForeachDataBind(javascriptDataSourceExpression);
                }
                else
                {
                    writer.WriteKnockoutForeachComment(javascriptDataSourceExpression);
                }
            }

            if (RenderWrapperTag)
            {
                base.RenderBeginTag(writer, context);
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            if (RenderWrapperTag)
            {
                base.RenderEndTag(writer, context);
            }

            if (!RenderOnServer && !RenderWrapperTag)
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (RenderOnServer)
            {
                // render on server
                foreach (var child in Children)
                {
                    child.Render(writer, context);
                }
            }
            else
            {
                // render on client
                var placeholder = new DataItemContainer() { DataContext = null };
                placeholder.SetValue(Internal.PathFragmentProperty, JavascriptCompilationHelper.AddIndexerToViewModel(GetPathFragmentExpression(), "$index"));
                placeholder.SetValue(Internal.ClientIDFragmentProperty, "'i' + $index()");
                Children.Add(placeholder);
                ItemTemplate.BuildContent(context.RequestContext, placeholder);

                placeholder.Render(writer, context);
            }
        }
    }
}
