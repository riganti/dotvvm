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
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("foreach", this, DataSourceProperty, () => { });
            }

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the contents inside the control begin and end tags.
        /// </summary>
        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var dataSourceBinding = GetBinding(DataSourceProperty) as ValueBindingExpression;

            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var item in DataSource)
                {
                    var placeholder = new DataItemContainer { DataContext = item, DataItemIndex = index };
                    Children.Add(placeholder);
                    ItemTemplate.BuildContent(placeholder);

                    context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[" + index + "]");
                    placeholder.Render(writer, context);
                    context.PathFragments.Pop();
                    index++;
                }
            }
            else
            {
                // render on client
                var placeholder = new DataItemContainer { DataContext = null };
                Children.Add(placeholder);
                ItemTemplate.BuildContent(placeholder);

                context.PathFragments.Push(dataSourceBinding.GetViewModelPathExpression(this, DataSourceProperty) + "[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();
            }
        }
    }
}
