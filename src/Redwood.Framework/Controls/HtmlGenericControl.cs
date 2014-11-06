using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;

namespace Redwood.Framework.Controls
{
    /// <summary>
    /// A control that represents plain HTML tag.
    /// </summary>
    public class HtmlGenericControl : RedwoodBindableControl, IControlWithHtmlAttributes
    {

        /// <summary>
        /// Gets the tag name.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public string TagName { get; protected set; }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public Dictionary<string, object> Attributes { get; private set; }


        /// <summary>
        /// Gets or sets whether the control is visible.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        public static readonly RedwoodProperty VisibleProperty =
            RedwoodProperty.Register<bool, HtmlGenericControl>(t => t.Visible, true);


        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl()
        {
            Attributes = new Dictionary<string, object>();            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl(string tagName) : this()
        {
            TagName = tagName;
        }


        /// <summary>
        /// Renders the control into the specified writer.
        /// </summary>
        public override void Render(IHtmlWriter writer, RenderContext context)
        {
            // handle visibility
            var visibleBinding = GetBinding(VisibleProperty);
            if (visibleBinding != null)
            {
                writer.AddKnockoutDataBind("visible", visibleBinding as ValueBindingExpression);
            }

            // render hard-coded HTML attributes
            foreach (var attribute in Attributes.Where(a => a.Value is string))
            {
                writer.AddAttribute(attribute.Key, attribute.Value.ToString());
            }

            // render binding HTML attributes
            var propertyValuePairs = Attributes.Where(a => a.Value is ValueBindingExpression)
                .Select(a => new KeyValuePair<string, ValueBindingExpression>(a.Key, (ValueBindingExpression)a.Value)).ToList();
            if (propertyValuePairs.Any())
            {
                writer.AddKnockoutDataBind("attr", propertyValuePairs);
            }

            writer.RenderBeginTag(TagName);
            base.Render(writer, context);
            writer.RenderEndTag();
        }
    }
}
