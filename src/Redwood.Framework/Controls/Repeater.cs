using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Repeats a specified template for each of the items in the <see cref="RedwoodBindableControl.DataContext"/> property.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false, DefaultContentProperty = "ItemTemplate")]
    public class Repeater : ItemsControl
    {

        /// <summary>
        /// Gets or sets the template for each <see cref="Repeater"/> item.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.InnerElement)]
        public ITemplate ItemTemplate
        {
            get { return (ITemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly RedwoodProperty ItemTemplateProperty =
            RedwoodProperty.Register<ITemplate, Repeater>(t => t.ItemTemplate, null);


        /// <summary>
        /// Gets or sets the name of the tag that wraps the Repeater.
        /// </summary>
        public string WrapperTagName
        {
            get { return (string)GetValue(WrapperTagNameProperty); }
            set { SetValue(WrapperTagNameProperty, value); }
        }
        public static readonly RedwoodProperty WrapperTagNameProperty =
            RedwoodProperty.Register<string, Repeater>(t => t.WrapperTagName, "div");

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater"/> class.
        /// </summary>
        public Repeater()
        {
        }


        /// <summary>
        /// Occurs after the viewmodel is applied to the page and before the commands are executed.
        /// </summary>
        protected internal override void OnLoad(RedwoodRequestContext context)
        {
            DataBind(context);
            base.OnLoad(context);
        }

        /// <summary>
        /// Occurs after the page commands are executed.
        /// </summary>
        protected internal override void OnPreRender(RedwoodRequestContext context)
        {
            DataBind(context);     // TODO: we should handle observable collection operations to persist controlstate of controls inside the Repeater
            base.OnPreRender(context);
        }

        /// <summary>
        /// Performs the data-binding and builds the controls inside the <see cref="Repeater"/>.
        /// </summary>
        private void DataBind(RedwoodRequestContext context)
        {
            Children.Clear();

            var dataSourceBinding = GetDataSourceBinding();
            var dataSourcePath = dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty);

            var index = 0;
            var dataSource = DataSource;
            if (dataSource != null)
            {
                foreach (var item in GetIEnumerableFromDataSource(dataSource))
                {
                    var placeholder = new DataItemContainer { DataItemIndex = index };
                    placeholder.SetBinding(DataContextProperty, new ValueBindingExpression(dataSourcePath + "[" + index + "]"));
                    Children.Add(placeholder);
                    ItemTemplate.BuildContent(context, placeholder);

                    index++;
                }
            }
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            TagName = WrapperTagName;

            if (!RenderOnServer)
            {
                writer.AddKnockoutForeachDataBind(GetDataSourceBinding().TranslateToClientScript(this, DataSourceProperty));
            }

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var dataSourceBinding = GetDataSourceBinding();

            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var child in Children)
                {
                    context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[" + index + "]");
                    Children[index].Render(writer, context);
                    context.PathFragments.Pop();
                    index++;
                }
            }
            else
            {
                // render on client
                var placeholder = new DataItemContainer { DataContext = null };
                placeholder.SetValue(Internal.IsDataContextBoundaryProperty, true);
                Children.Add(placeholder);
                ItemTemplate.BuildContent(context.RequestContext, placeholder);

                context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();
            }
        }
    }
}
