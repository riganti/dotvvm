using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding.Expressions;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A control that represents plain HTML tag.
    /// </summary>
    public class HtmlGenericControl : DotvvmControl, IControlWithHtmlAttributes
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

        /// <summary>
        /// Gets or sets the inner text of the HTML element. 
        /// </summary>
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
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            object id;
            if (!IsPropertySet(ClientIDProperty))
            {
                SetValueRaw(ClientIDProperty, id = CreateClientId());
            }
            else
            {
                id = GetValueRaw(ClientIDProperty);
            }
            if (id != null) Attributes["id"] = id;

            CheckInnerTextUsage();

            // verify that the properties are used only where they should
            if (!RendersHtmlTag)
            {
                EnsureNoAttributesSet();
            }
            else
            {
                var attrBindingGroup = new KnockoutBindingGroup();
                // render hard-coded HTML attributes
                foreach (var attribute in Attributes)
                {
                    if (attribute.Value is IValueBinding)
                    {
                        var binding = attribute.Value as IValueBinding;
                        attrBindingGroup.Add(attribute.Key, binding.GetKnockoutBindingExpression());
                        if (!RenderOnServer)
                            continue;
                    }
                    AddHtmlAttribute(writer, attribute.Key, attribute.Value);
                }

                if (!attrBindingGroup.IsEmpty)
                {
                    writer.AddKnockoutDataBind("attr", attrBindingGroup);
                }

                // handle Visible property
                AddVisibleAttributeOrBinding(writer);

                // handle Text property
                writer.AddKnockoutDataBind("text", this, InnerTextProperty, () =>
                {
                    // inner Text is rendered as attribute only if contains binding
                    // otherwise it is rendered directly as encoded content
                    if (!string.IsNullOrWhiteSpace(InnerText))
                    {
                        Children.Clear();
                        Children.Add(new Literal(InnerText));
                    }
                });
            }

            base.AddAttributesToRender(writer, context);
        }

        void AddHtmlAttribute(IHtmlWriter writer, string name, object value)
        {
            if (value is string || value == null)
            {
                writer.AddAttribute(name, (string)value, true);
            }
            else if (value is IEnumerable<string>)
            {
                foreach (var vv in (IEnumerable<string>)value)
                {
                    writer.AddAttribute(name, vv);
                }
            }
            else if (value is IStaticValueBinding)
            {
                AddHtmlAttribute(writer, name, ((IStaticValueBinding)value).Evaluate(this, null));
            }
            else throw new NotSupportedException($"Attribute value of type '{value.GetType().FullName}' is not supported.");
        }

        /// <summary>
        /// Checks the inner text property usage.
        /// </summary>
        private void CheckInnerTextUsage()
        {
            if (GetType() != typeof(HtmlGenericControl))
            {
                if (GetValueRaw(InnerTextProperty) != null)
                {
                    throw new DotvvmControlException(this, "The DotVVM controls do not support the 'InnerText' property. It can be only used on HTML elements.");
                }
            }
        }

        /// <summary>
        /// Adds the corresponding attribute or binding for the Visible property.
        /// </summary>
        protected virtual void AddVisibleAttributeOrBinding(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("visible", this, VisibleProperty, renderEvenInServerRenderingMode: true);
            if (GetValue(VisibleProperty) as bool? == false)
            {
                writer.AddStyleAttribute("display", "none");
            }
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RendersHtmlTag) writer.RenderBeginTag(TagName);
        }

        /// <summary>
        /// Renders the control end tag.
        /// </summary>
        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RendersHtmlTag) writer.RenderEndTag();
        }

        /// <summary>
        /// Verifies that the control hasn't any HTML attributes or Visible or DataContext bindings set.
        /// </summary>
        protected virtual void EnsureNoAttributesSet()
        {
            if (Attributes.Any() || IsPropertySet(VisibleProperty) || HasBinding(DataContextProperty))
            {
                throw new DotvvmControlException(this, "Cannot set HTML attributes, Visible, DataContext, ID, Postback.Update, ... bindings on a control which does not render its own element!");
            }
        }

        /// <summary>
        /// Gets a value whether this control renders a HTML tag.
        /// </summary>
        protected virtual bool RendersHtmlTag => true;

    }
}
