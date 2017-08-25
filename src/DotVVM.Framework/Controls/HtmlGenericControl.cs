using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// A control that represents plain HTML tag.
    /// </summary>
    public class HtmlGenericControl : DotvvmControl, IControlWithHtmlAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl()
        {
            Attributes = new Dictionary<string, object>();
            if (GetType() == typeof(HtmlGenericControl))
            {
                LifecycleRequirements = ControlLifecycleRequirements.None;
            }
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
        /// Gets the attributes.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Attribute, AllowBinding = true, AllowHardCodedValue = true, AllowValueMerging = true, AttributeValueMerger = typeof(HtmlAttributeValueMerger), AllowAttributeWithoutValue = true)]
        [PropertyGroup(new[] { "", "html:" })]
        public Dictionary<string, object> Attributes { get; private set; }

        public VirtualPropertyGroupDictionary<string> CssClasses => new VirtualPropertyGroupDictionary<string>(this, CssClassesGroupDescriptor);

        public static DotvvmPropertyGroup CssClassesGroupDescriptor =
            DotvvmPropertyGroup.Register<string, HtmlGenericControl>("Class-", "CssClasses");

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
        /// Gets the tag name.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public string TagName { get; protected set; }

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
        /// Gets a value whether this control renders a HTML tag.
        /// </summary>
        protected virtual bool RendersHtmlTag => true;

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            AddClientIdAttribute();
            CheckInnerTextUsage();

            if (!RendersHtmlTag)
            {
                // the control renders no html tag and therefore if it has any attributes it should throw an exception
                EnsureNoAttributesSet();
            }
            else
            {
                AddHtmlAttributesToRender(writer);
                AddCssClassesToRender(writer);
                AddVisibleAttributeOrBinding(writer);
                AddTextPropertyToRender(writer);
            }

            base.AddAttributesToRender(writer, context);
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
        /// Verifies that the control hasn't any HTML attributes or Visible or DataContext bindings set.
        /// </summary>
        protected virtual void EnsureNoAttributesSet()
        {
            if (Attributes.Count > 0 || CssClasses.Count > 0 || !true.Equals(GetValueRaw(VisibleProperty)) || HasBinding(DataContextProperty))
            {
                throw new DotvvmControlException(this, "Cannot set HTML attributes, Visible, ID, Postback.Update, ... bindings on a control which does not render its own element!");
            }
        }

        /// <summary>
        /// Renders the control begin tag.
        /// </summary>
        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RendersHtmlTag)
            {
                writer.RenderBeginTag(TagName);
            }
        }

        /// <summary>
        /// Renders the control end tag.
        /// </summary>
        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RendersHtmlTag)
            {
                writer.RenderEndTag();
            }
        }

        private void AddClientIdAttribute()
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
        }

        private void AddCssClassesToRender(IHtmlWriter writer)
        {
            KnockoutBindingGroup cssClassBindingGroup = null;
            foreach (var cssClass in CssClasses.Properties)
            {
                if (cssClassBindingGroup == null) cssClassBindingGroup = new KnockoutBindingGroup();
                cssClassBindingGroup.Add(cssClass.GroupMemberName, this, cssClass, null);
            }
            if (cssClassBindingGroup != null) writer.AddKnockoutDataBind("css", cssClassBindingGroup);
        }

        private void AddHtmlAttribute(IHtmlWriter writer, string name, object value)
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
                AddHtmlAttribute(writer, name, ((IStaticValueBinding)value).Evaluate(this));
            }
            else throw new NotSupportedException($"Attribute value of type '{value.GetType().FullName}' is not supported.");
        }

        private void AddHtmlAttributesToRender(IHtmlWriter writer)
        {
            var attributeBindingGroup = new KnockoutBindingGroup();
            foreach (var attribute in Attributes)
            {
                if (attribute.Value is IValueBinding)
                {
                    var binding = attribute.Value as IValueBinding;
                    attributeBindingGroup.Add(attribute.Key, binding.GetKnockoutBindingExpression(this));
                    if (!RenderOnServer)
                        continue;
                }
                AddHtmlAttribute(writer, attribute.Key, attribute.Value);
            }
            if (!attributeBindingGroup.IsEmpty)
            {
                writer.AddKnockoutDataBind("attr", attributeBindingGroup);
            }
        }

        private void AddTextPropertyToRender(IHtmlWriter writer)
        {
            writer.AddKnockoutDataBind("text", this, InnerTextProperty, () =>
            {
                // inner Text is rendered as attribute only if contains binding
                // otherwise it is rendered directly as encoded content
                if (!string.IsNullOrWhiteSpace(InnerText))
                {
                    Children.Clear();
                    Children.Add(new Literal(InnerText));
                }
            }, renderEvenInServerRenderingMode: true);
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
    }
}