using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

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

        public string InnerText
        {
            get { return (string)GetValue(InnerTextProperty); }
            set { SetValue(InnerTextProperty, value); }
        }
        public static readonly RedwoodProperty InnerTextProperty =
            RedwoodProperty.Register<string, HtmlGenericControl>(t => t.InnerText, null);
    
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

            if (tagName == "head")
            {
                SetValue(RenderSettings.ModeProperty, RenderMode.Server);
            }
        }


        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            // render hard-coded HTML attributes
            foreach (var attribute in Attributes.Where(a => a.Value is string || a.Value == null))
            {
                writer.AddAttribute(attribute.Key, (string)attribute.Value, true);
            }

            // render binding HTML attributes
            var propertyValuePairs = Attributes.Where(a => a.Value is ValueBindingExpression)
                .Select(a => new KeyValuePair<string, ValueBindingExpression>(a.Key, (ValueBindingExpression)a.Value)).ToList();
            if (propertyValuePairs.Any())
            {
                writer.AddKnockoutDataBind("attr", propertyValuePairs, this, null);
            }

            // handle Visible property
            writer.AddKnockoutDataBind("visible", this, VisibleProperty, () =>
            {
                if (!Visible)
                {
                    writer.AddStyleAttribute("display", "none");
                }
            });

            // handle Text property
            writer.AddKnockoutDataBind("text", this, InnerTextProperty, () =>
            {
                // inner Text is rendered as attribute only if contains binding
                // otherwise it is rendered directly as encoded content
            });

            // hadle Id property
            if (!string.IsNullOrEmpty(ID))
            {
                writer.AddAttribute("id", ID);
            }

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected override void RenderBeginTag(IHtmlWriter writer, RenderContext context)
        {
            writer.RenderBeginTag(TagName);
        }

        /// <summary>
        /// Renders the control end tag.
        /// </summary>
        protected override void RenderEndTag(IHtmlWriter writer, RenderContext context)
        {
            writer.RenderEndTag();
        }

    }
}
