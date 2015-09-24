using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A control that represents plain HTML tag.
    /// </summary>
    public class HtmlGenericControl : DotvvmBindableControl, IControlWithHtmlAttributes
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
        public static readonly DotvvmProperty VisibleProperty =
            DotvvmProperty.Register<bool, HtmlGenericControl>(t => t.Visible, true);

        public string InnerText
        {
            get { return (string)GetValue(InnerTextProperty); }
            set { SetValue(InnerTextProperty, value); }
        }
        public static readonly DotvvmProperty InnerTextProperty =
            DotvvmProperty.Register<string, HtmlGenericControl>(t => t.InnerText, null);

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl()
        {
            Attributes = new Dictionary<string, object>();
        }

        /// <summary> ahoj kokoti
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

            foreach (var attrList in Attributes.Where(a => a.Value is string[]))
            {
                foreach (var value in (string[])attrList.Value)
                {
                    writer.AddAttribute(attrList.Key, value);
                }
            }

            // render binding HTML attributes
            var propertyValuePairs = Attributes.Where(a => a.Value is IValueBinding)
                .Select(a => new KeyValuePair<string, IValueBinding>(a.Key, (IValueBinding)a.Value)).ToList();
            if (!RenderOnServer)
            {
                if (propertyValuePairs.Any())
                {
                    writer.AddKnockoutDataBind("attr", propertyValuePairs, this, null);
                }
            }
            else
            {
                foreach (var prop in propertyValuePairs)
                {
                    writer.AddAttribute(prop.Key, prop.Value.Evaluate(this, null)?.ToString());
                }
            }

            // handle Visible property
            AddVisibleAttributeOrBinding(writer);

            // handle Text property
            writer.AddKnockoutDataBind("text", this, InnerTextProperty, () =>
            {
                // inner Text is rendered as attribute only if contains binding
                // otherwise it is rendered directly as encoded content
            });

            // hadle Id property
            AddIdAttribute(writer);

            base.AddAttributesToRender(writer, context);
        }

        /// <summary>
        /// Adds the corresponding attribute or binding for the Visible property.
        /// </summary>
        protected virtual void AddVisibleAttributeOrBinding(IHtmlWriter writer)
        {
            var visibleBinding = GetValueBinding(VisibleProperty);
            if (visibleBinding != null && !RenderOnServer)
            {
                writer.AddKnockoutDataBind("visible", this, VisibleProperty);
                writer.AddStyleAttribute("display", "none");
            }
            else
            {
                if (!Visible)
                {
                    writer.AddStyleAttribute("display", "none");
                }
            }
        }

        /// <summary>
        /// Adds the corresponding attribute for the Id property.
        /// </summary>
        protected virtual void AddIdAttribute(IHtmlWriter writer)
        {
            if (!string.IsNullOrEmpty(ID))
            {
                writer.AddAttribute("id", ID);
            }
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

        /// <summary>
        /// Verifies that the control hasn't any HTML attributes or Visible or DataContext bindings set.
        /// </summary>
        protected void EnsureNoAttributesSet()
        {
            if (Attributes.Any() || HasBinding(VisibleProperty) || HasBinding(DataContextProperty))
            {
                throw new Exception("Cannot set HTML attributes, Visible or DataContext bindings on a control which doesn't render its own element!");   // TODO
            }
        }
    }
}
