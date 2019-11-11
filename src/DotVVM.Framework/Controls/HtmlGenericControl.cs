#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
#pragma warning disable CS8618 // TagName should not be null, but unfortunately is not initialized
        public HtmlGenericControl(bool allowImplicitLifecycleRequirements = true)
#pragma warning restore CS8618
        {
            Attributes = new Dictionary<string, object?>();
            if (allowImplicitLifecycleRequirements && GetType() == typeof(HtmlGenericControl))
            {
                LifecycleRequirements = ControlLifecycleRequirements.None;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl(string? tagName, bool allowImplicitLifecycleRequirements = true) : this(allowImplicitLifecycleRequirements)
        {
            if (tagName?.Trim() == "")
            {
                throw new DotvvmControlException("The tagName must not be empty!");
            }

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
        public Dictionary<string, object?> Attributes { get; private set; }

        public VirtualPropertyGroupDictionary<bool> CssClasses => new VirtualPropertyGroupDictionary<bool>(this, CssClassesGroupDescriptor);

        public static DotvvmPropertyGroup CssClassesGroupDescriptor =
            DotvvmPropertyGroup.Register<bool, HtmlGenericControl>("Class-", nameof(CssClasses));

        public VirtualPropertyGroupDictionary<object> CssStyles => new VirtualPropertyGroupDictionary<object>(this, CssStylesGroupDescriptor);

        public static DotvvmPropertyGroup CssStylesGroupDescriptor =
            DotvvmPropertyGroup.Register<object, HtmlGenericControl>("Style-", nameof(CssStyles));

        /// <summary>
        /// Gets or sets the inner text of the HTML element.
        /// </summary>
        public string? InnerText
        {
            get { return (string?)GetValue(InnerTextProperty); }
            set { SetValue(InnerTextProperty, value); }
        }

        public static readonly DotvvmProperty InnerTextProperty =
            DotvvmProperty.Register<string?, HtmlGenericControl>(t => t.InnerText, null);

        /// <summary>
        /// Gets the tag name.
        /// </summary>
        [MarkupOptions(MappingMode = MappingMode.Exclude)]
        public string? TagName { get; protected set; }

        /// <summary>
        /// Gets or sets whether the control is visible.
        /// </summary>
        [MarkupOptions(AllowHardCodedValue = false)]
        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty)!; }
            set { SetValue(VisibleProperty, value); }
        }

        public static readonly DotvvmProperty VisibleProperty =
            DotvvmProperty.Register<bool, HtmlGenericControl>(t => t.Visible, true);

        /// <summary>
        /// Gets a value whether this control renders a HTML tag.
        /// </summary>
        protected virtual bool RendersHtmlTag => TagName is object;

        protected new struct RenderState
        {
            public object? Visible;
            public object? ClientId;
            public object? InnerText;
            public bool HasId;
            public bool HasClass;
            public bool HasStyle;
            public bool HasPostbackUpdate;
            public bool RendersHtmlTag;

            private byte _renderOnServer;

            public bool RenderOnServer(HtmlGenericControl @this)
            {
                if (_renderOnServer == 0)
                    _renderOnServer = @this.RenderOnServer ? (byte)1 : (byte)2;
                return _renderOnServer == 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool TouchProperty(DotvvmProperty prop, object? value, ref RenderState r)
        {
            if (prop == VisibleProperty)
                r.Visible = value;
            else if (prop == ClientIDProperty)
                r.ClientId = value;
            else if (prop == IDProperty)
                r.HasId = true;
            else if (prop == InnerTextProperty)
                r.InnerText = value;
            else if (prop == PostBack.UpdateProperty)
                r.HasPostbackUpdate = (bool)this.EvalPropertyValue(prop, value)!;
            else if (prop is GroupedDotvvmProperty gp)
            {
                if (gp.PropertyGroup == CssClassesGroupDescriptor)
                    r.HasClass = true;
                else if (gp.PropertyGroup == CssStylesGroupDescriptor)
                    r.HasStyle = true;
                else return false;
            }
            else return false;
            return true;
        }

        /// <summary>
        /// Adds all attributes that should be added to the control begin tag.
        /// </summary>
        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderState r = default;
            foreach (var (prop, val) in this.properties)
                TouchProperty(prop, val, ref r);
            r.RendersHtmlTag = this.RendersHtmlTag;

            AddAttributesCore(writer, ref r);

            base.AddAttributesToRender(writer, context);
        }

        protected void AddAttributesCore(IHtmlWriter writer, ref RenderState r)
        {
            AddClientIdAttribute(ref r);
            CheckInnerTextUsage(in r);

            if (!r.RendersHtmlTag)
            {
                // the control renders no html tag and therefore if it has any attributes it should throw an exception
                EnsureNoAttributesSet(in r);
            }
            else
            {
                if (r.HasClass)
                    AddCssClassesToRender(writer);
                if (r.HasStyle)
                    AddCssStylesToRender(writer);
                AddVisibleAttributeOrBinding(in r, writer);
                AddTextPropertyToRender(ref r, writer);
                AddHtmlAttributesToRender(ref r, writer);
            }
        }

        /// <summary>
        /// Adds the corresponding attribute or binding for the Visible property.
        /// </summary>
        protected virtual void AddVisibleAttributeOrBinding(in RenderState r, IHtmlWriter writer)
        {
            var v = r.Visible;
            if (v is IValueBinding binding)
                writer.AddKnockoutDataBind("visible", binding.GetKnockoutBindingExpression(this));

            if (false.Equals(EvalPropertyValue(VisibleProperty, v)))
            {
                writer.AddAttribute("style", "display:none");
            }
        }

        /// <summary>
        /// Verifies that the control hasn't any HTML attributes, css classes or Visible or DataContext bindings set.
        /// </summary>
        private void EnsureNoAttributesSet(in RenderState r)
        {
            if (Attributes.Count > 0 || r.HasClass || r.HasStyle || (r.Visible != null && !true.Equals(r.Visible)) || r.HasPostbackUpdate)
            {
                throw new DotvvmControlException(this, "Cannot set HTML attributes, Visible, ID, Postback.Update, ... bindings on a control which does not render its own element!");
            }
        }

        /// <summary>
        /// Renders the control begin tag if <see cref="RendersHtmlTag" /> == true.
        /// </summary>
        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RendersHtmlTag)
            {
                writer.RenderBeginTag(TagName!);
            }
        }

        /// <summary>
        /// Renders the control end tag if <see cref="RendersHtmlTag" /> == true. Also renders required resource i before the end tag, if it is a `head` or `body` element
        /// </summary>
        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // render resource link. If the Render is invoked multiple times the resources are rendered on the first invocation.
            if (TagName == "head")
                new HeadResourceLinks().Render(writer, context);
            else if (TagName == "body")
                new BodyResourceLinks().Render(writer, context);

            if (RendersHtmlTag)
            {
                writer.RenderEndTag();
            }
        }

        private void AddClientIdAttribute(ref RenderState r)
        {
            if (r.HasId && r.ClientId == null)
            {
                SetValueRaw(ClientIDProperty, r.ClientId = CreateClientId());
            }

            if (r.ClientId != null) Attributes["id"] = r.ClientId;
        }

        private void AddCssClassesToRender(IHtmlWriter writer)
        {
            KnockoutBindingGroup cssClassBindingGroup = new KnockoutBindingGroup();
            foreach (var cssClass in CssClasses.Properties)
            {
                if (HasValueBinding(cssClass))
                {
                    cssClassBindingGroup.Add(cssClass.GroupMemberName, this, cssClass);
                }

                try
                {
                    if (true.Equals(this.GetValue(cssClass)))
                        writer.AddAttribute("class", cssClass.GroupMemberName, append: true, appendSeparator: " ");
                }
                catch when (HasValueBinding(cssClass)) { }
            }

            if (!cssClassBindingGroup.IsEmpty) writer.AddKnockoutDataBind("css", cssClassBindingGroup);
        }

        private void AddCssStylesToRender(IHtmlWriter writer)
        {
            KnockoutBindingGroup? cssStylesBindingGroup = null;
            foreach (var styleProperty in CssStyles.Properties)
            {
                if (HasValueBinding(styleProperty))
                {
                    if (cssStylesBindingGroup == null) cssStylesBindingGroup = new KnockoutBindingGroup();
                    cssStylesBindingGroup.Add(styleProperty.GroupMemberName, this, styleProperty);
                }

                try
                {
                    var value = GetValue(styleProperty)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        writer.AddStyleAttribute(styleProperty.GroupMemberName, value!);
                    }
                }
                // suppress all errors when we have rendered the value binding anyway
                catch when (HasValueBinding(styleProperty)) { }
            }

            if (cssStylesBindingGroup != null)
            {
                writer.AddKnockoutDataBind("style", cssStylesBindingGroup);
            }
        }

        private void AddHtmlAttribute(IHtmlWriter writer, string name, object? value)
        {
            if (value is string || value == null)
            {
                writer.AddAttribute(name, (string?)value, true);
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

        private void AddHtmlAttributesToRender(ref RenderState r, IHtmlWriter writer)
        {
            KnockoutBindingGroup? attributeBindingGroup = null;
            foreach (var attribute in Attributes)
            {
                if (attribute.Value is IValueBinding binding)
                {
                    if (attributeBindingGroup == null) attributeBindingGroup = new KnockoutBindingGroup();
                    attributeBindingGroup.Add(attribute.Key, binding.GetKnockoutBindingExpression(this));
                    if (!r.RenderOnServer(this))
                        continue;
                }
                AddHtmlAttribute(writer, attribute.Key, attribute.Value);
            }
            if (attributeBindingGroup != null)
            {
                writer.AddKnockoutDataBind("attr", attributeBindingGroup);
            }
        }

        private void AddTextPropertyToRender(ref RenderState r, IHtmlWriter writer)
        {
            if (r.InnerText == null) return;
            var expression = r.InnerText as IValueBinding;
            if (expression != null)
            {
                writer.AddKnockoutDataBind("text", expression.GetKnockoutBindingExpression(this));
            }

            var value = (string?)this.EvalPropertyValue(InnerTextProperty, r.InnerText);
            if ((expression == null && !string.IsNullOrWhiteSpace(value))
                || r.RenderOnServer(this))
            {
                Children.Clear();
                if (value is object)
                    Children.Add(new Literal(value));
            }
        }

        /// <summary>
        /// Checks the inner text property usage.
        /// </summary>
        private void CheckInnerTextUsage(in RenderState r)
        {
            if (r.InnerText != null && GetType() != typeof(HtmlGenericControl))
            {
                throw new DotvvmControlException(this, "The DotVVM controls do not support the 'InnerText' property. It can be only used on HTML elements.");
            }
        }
    }
}
