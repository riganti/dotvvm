using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Repeats a specified template for each of the items in the <see cref="RedwoodBindableControl.DataContext"/> property.
    /// </summary>
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
            get { return TagName; }
            set { TagName = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Repeater"/> class.
        /// </summary>
        public Repeater()
        {
            TagName = "div";
        }



        /// <summary>
        /// Renders the children.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                var dataSourceBinding = GetDataSourceBinding();
                writer.AddKnockoutDataBind("foreach", dataSourceBinding as ValueBindingExpression, this, DataSourceProperty);
            }

            base.Render(writer, context);
        }

        /// <summary>
        /// Renders the children.
        /// </summary>
        protected override void RenderChildren(IHtmlWriter writer, RenderContext context)
        {
            var dataSourceBinding = GetBinding(DataSourceProperty);

            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var item in DataSource)
                {
                    var placeholder = new DataItemContainer { DataContext = item, DataItemIndex = index };
                    ItemTemplate.BuildContent(placeholder);

                    context.PathFragments.Push(dataSourceBinding.Expression);
                    context.PathFragments.Push("[" + index + "]");
                    placeholder.Render(writer, context);
                    context.PathFragments.Pop();
                    context.PathFragments.Pop();
                    index++;
                }
            }
            else
            {
                // render on client
                var placeholder = new DataItemContainer { DataContext = null };
                ItemTemplate.BuildContent(placeholder);

                context.PathFragments.Push(dataSourceBinding.Expression);
                context.PathFragments.Push("[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();
                context.PathFragments.Pop();
            }
        }
    }
}
