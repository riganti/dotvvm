using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

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
            if (tagName is not null && string.IsNullOrWhiteSpace(tagName))
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
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl(string? tagName, HtmlCapability? html, bool allowImplicitLifecycleRequirements = true) : this(tagName, allowImplicitLifecycleRequirements)
        {
            if (html is {})
                HtmlCapabilityProperty.SetValue(this, html);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenericControl"/> class.
        /// </summary>
        public HtmlGenericControl(string? tagName, TextOrContentCapability? content, HtmlCapability? html = null)
        {
            if (GetType() != typeof(HtmlGenericControl))
                throw new("HtmlGenericControl can only use InnerText (and thus TextOrContentCapability) property when used directly, it cannot be inherited.");
            if (tagName?.Trim() == "")
            {
                throw new DotvvmControlException("The tagName must not be empty!");
            }

            TagName = tagName;

            if (tagName == "head")
            {
                SetValue(RenderSettings.ModeProperty, RenderMode.Server);
            }
            LifecycleRequirements = ControlLifecycleRequirements.None;

            content?.WriteToChildren(this, InnerTextProperty);

            if (html is {})
                HtmlCapabilityProperty.SetValue(this, html);
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        [PropertyGroup(new[] { "", "html:" })]
        public VirtualPropertyGroupDictionary<object?> Attributes => new(this, AttributesGroupDescriptor);

        [MarkupOptions(MappingMode = MappingMode.Attribute, AllowBinding = true, AllowHardCodedValue = true, AllowValueMerging = true, AttributeValueMerger = typeof(HtmlAttributeValueMerger), AllowAttributeWithoutValue = true)]
        public static DotvvmPropertyGroup AttributesGroupDescriptor =
            DotvvmPropertyGroup.Register<object, HtmlGenericControl>(new [] { "", "html:" }, nameof(Attributes));

        [PropertyGroup("Class-", ValueType = typeof(bool))]
        public VirtualPropertyGroupDictionary<bool> CssClasses => new(this, CssClassesGroupDescriptor);

        public static DotvvmPropertyGroup CssClassesGroupDescriptor =
            DotvvmPropertyGroup.Register<bool, HtmlGenericControl>("Class-", nameof(CssClasses));

        [PropertyGroup("Style-")]
        public VirtualPropertyGroupDictionary<object> CssStyles => new(this, CssStylesGroupDescriptor);

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


        public HtmlCapability HtmlCapability
        {
            get => (HtmlCapability)this.GetValue(HtmlCapabilityProperty)!;
            set => this.SetValue(HtmlCapabilityProperty, value);
        }
        public static readonly DotvvmCapabilityProperty HtmlCapabilityProperty =
            DotvvmCapabilityProperty.RegisterCapability<HtmlCapability, HtmlGenericControl>();

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
            public bool HasAttributes;
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
            else if (prop == IDProperty && value != null)
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
                else if (gp.PropertyGroup == AttributesGroupDescriptor)
                    r.HasAttributes = true;
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
            CheckInnerTextUsage(in r);

            if (!r.RendersHtmlTag)
            {
                // the control renders no html tag and therefore if it has any attributes it should throw an exception
                EnsureNoAttributesSet(in r);
            }
            else
            {
                TryUseLiteralAsInnerText(ref r);

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
            if (r.HasAttributes || r.HasClass || r.HasStyle || (r.Visible != null && !true.Equals(r.Visible)) || r.HasPostbackUpdate)
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
            else if (value is bool boolValue)
            {
                if (boolValue)
                {
                    writer.AddAttribute(name, name);
                }
            }
            else if (value is Enum enumValue)
            {
                writer.AddAttribute(name, enumValue.ToEnumString());
            }
            else if (value is Guid)
            {
                writer.AddAttribute(name, value.ToString());
            }
            else if (ReflectionUtils.IsNumericType(value.GetType()))
            {
                writer.AddAttribute(name, Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            else
            {
                // DateTime and related are not supported here intentionally.
                // It is not clear in which format it should be rendered - on some places, the HTML specs requires just yyyy-MM-dd,
                // but in case of Web Components, the users may want to pass the whole date, or use a specific format

                throw new NotSupportedException($"Attribute value of type '{value.GetType().FullName}' is not supported. Please convert the value to string, e. g. by using ToString()");
            }
        }

        private void AddHtmlAttributesToRender(ref RenderState r, IHtmlWriter writer)
        {
            KnockoutBindingGroup? attributeBindingGroup = null;
            if (r.HasAttributes) foreach (var (prop, valueRaw) in this.properties)
            {
                if (prop is not GroupedDotvvmProperty gprop || gprop.PropertyGroup != AttributesGroupDescriptor)
                    continue;

                if (valueRaw is IValueBinding binding)
                {
                    if (gprop.GroupMemberName == "class")
                    {
                        writer.AddKnockoutDataBind("class", binding, this);
                    }
                    else
                    {
                        if (attributeBindingGroup == null) attributeBindingGroup = new KnockoutBindingGroup();
                        attributeBindingGroup.Add(gprop.GroupMemberName, binding.GetKnockoutBindingExpression(this));
                    }
                    if (!r.RenderOnServer(this))
                        continue;
                }
                AddHtmlAttribute(writer, gprop.GroupMemberName, valueRaw);
            }

            if (r.HasId)
            {
                var clientId = r.ClientId ?? CreateClientId();
                if (clientId is IValueBinding binding)
                {
                    if (attributeBindingGroup == null) attributeBindingGroup = new KnockoutBindingGroup();
                    attributeBindingGroup.Add("id", binding.GetKnockoutBindingExpression(this));
                }
                else
                {
                    // TODO: we currently don't support server-side rendering of value binding IDs
                    AddHtmlAttribute(writer, "id", clientId);
                }
            }

            if (attributeBindingGroup != null)
            {
                writer.AddKnockoutDataBind("attr", attributeBindingGroup);
            }
        }

        /// Tries to get Literal element from Children and set its value binding into r.InnerText
        /// This leads to less knockout comments being produced
        void TryUseLiteralAsInnerText(ref RenderState r)
        {
            if (r.InnerText != null || Children.Count != 1)
                return;
            if (Children[0] is not Literal { RendersHtmlTag: false, FormatString: null or "" } literal)
                return;

            var textBinding = literal.GetValueRaw(Literal.TextProperty) as IValueBinding;
            if (textBinding is null || Literal.NeedsFormatting(textBinding))
                return;

            Children.Clear();
            r.InnerText = textBinding;
        }

        private void AddTextPropertyToRender(ref RenderState r, IHtmlWriter writer)
        {
            if (r.InnerText == null) return;
            var expression = r.InnerText as IValueBinding;
            if (expression != null)
            {
                writer.AddKnockoutDataBind("text", expression.GetKnockoutBindingExpression(this));
            }

            if (expression == null || r.RenderOnServer(this))
            {
                var value = this.EvalPropertyValue(InnerTextProperty, r.InnerText)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    Children.Clear();
                    Children.Add(new Literal(value));
                }
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


    [DotvvmControlCapability]
    public sealed record HtmlCapability
    {
        [PropertyGroup("", "html:")]
        [MarkupOptions(MappingMode = MappingMode.Attribute, AllowBinding = true, AllowHardCodedValue = true, AllowValueMerging = true, AttributeValueMerger = typeof(HtmlAttributeValueMerger), AllowAttributeWithoutValue = true)]
        public IDictionary<string, ValueOrBinding<object?>> Attributes { get; init; } = new Dictionary<string, ValueOrBinding<object?>>();
        [PropertyGroup("Class-")]
        public IDictionary<string, ValueOrBinding<bool>> CssClasses { get; init; } = new Dictionary<string, ValueOrBinding<bool>>();
        [PropertyGroup("Style-")]
        public IDictionary<string, ValueOrBinding<object>> CssStyles { get; init; } = new Dictionary<string, ValueOrBinding<object>>();
        public ValueOrBinding<bool> Visible { get; init; } = new(true);

        public ValueOrBinding<string?> ID { get; init; }
    }
}
