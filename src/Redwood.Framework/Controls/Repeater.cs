using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// Repeats a specified template for each of the items in the <see cref="RedwoodBindableControl.DataContext"/> property.
    /// </summary>
    public class Repeater : RedwoodBindableControl
    {

        /// <summary>
        /// Gets or sets whether the contents of the control are rendered on the server.
        /// </summary>
        public bool RenderOnServer
        {
            get { return (bool)GetValue(RenderOnServerProperty); }
            set { SetValue(RenderOnServerProperty, value); }
        }
        public static readonly RedwoodProperty RenderOnServerProperty =
            RedwoodProperty.Register<bool, Repeater>(t => t.RenderOnServer, false);



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
        /// Renders the children.
        /// </summary>
        protected override void RenderChildren(IHtmlWriter writer, RenderContext context)
        {
            if (!(DataContext is IEnumerable))
            {
                throw new Exception("The DataContext of the Repeater control must implement the IEnumerable interface!");   // TODO: exception handling
            }

            if (RenderOnServer)
            {
                // render on server
                var index = 0;
                foreach (var item in (IEnumerable)DataContext)
                {
                    var placeholder = new DataItemContainer { DataContext = item };
                    ItemTemplate.BuildContent(placeholder);

                    context.PathFragments.Push("[" + index + "]");
                    placeholder.Render(writer, context);
                    context.PathFragments.Pop();
                    index++;
                }
            }
            else
            {
                var dataContextBinding = GetBinding(DataContextProperty);
                if (dataContextBinding == null)
                {
                    throw new Exception("The DataContext property must contain a binding!");    // TODO: exception handling
                }
                writer.AddKnockoutDataBind("foreach", dataContextBinding as ValueBindingExpression);
                writer.RenderBeginTag(WrapperTagName);

                // render on client
                var placeholder = new DataItemContainer { DataContext = null };
                ItemTemplate.BuildContent(placeholder);

                context.PathFragments.Push("[$index]");
                placeholder.Render(writer, context);
                context.PathFragments.Pop();

                writer.RenderEndTag();
            }
        }
    }
}
