using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Binding.Properties;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Renders a text into the page.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public sealed class Literal : HtmlGenericControl
    {
        /// <summary>
        /// Gets or sets the text displayed in the control.
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty)?.ToString() ?? ""; }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<object, Literal>(t => t.Text, "");

        /// <summary>
        /// Gets or sets the format string that will be applied to numeric or date-time values.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? FormatString
        {
            get { return (string?)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, Literal>(c => c.FormatString, "");

        /// <summary>
        /// Gets or sets the type of value being formatted - Number or DateTime.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        [Obsolete("ValueType property is no longer required, it is automatically inferred from compile-time type of Text binding")]
        public FormatValueType ValueType
        {
            get { return (FormatValueType)GetValue(ValueTypeProperty)!; }
            set { SetValue(ValueTypeProperty, value); }
        }

        [Obsolete("ValueType property is no longer required, it is automatically inferred from compile-time type of Text binding")]
        public static readonly DotvvmProperty ValueTypeProperty =
            DotvvmProperty.Register<FormatValueType, Literal>(t => t.ValueType);

        /// <summary>
        /// Gets or sets whether the literal should render the wrapper span HTML element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderSpanElement
        {
            get { return (bool)GetValue(RenderSpanElementProperty)!; }
            set { SetValue(RenderSpanElementProperty, value); }
        }

        public static readonly DotvvmProperty RenderSpanElementProperty =
            DotvvmProperty.Register<bool, Literal>(t => t.RenderSpanElement, true);

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(bool allowImplicitLifecycleRequirements = true) : base("span")
        {
            if (allowImplicitLifecycleRequirements) LifecycleRequirements = ControlLifecycleRequirements.PreRender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(string text, bool allowImplicitLifecycleRequirements = true) : base("span", false)
        {
            Text = text;
            RenderSpanElement = false;
            if (allowImplicitLifecycleRequirements) LifecycleRequirements = ControlLifecycleRequirements.None;
        }

        public Literal(ValueOrBinding text, bool renderSpan = false): this()
        {
            SetValue(TextProperty, text);
            RenderSpanElement = renderSpan;
        }

        public Literal(IStaticValueBinding text, bool renderSpan = false): this()
        {
            SetBinding(TextProperty, text);
            RenderSpanElement = renderSpan;
        }

        public static bool NeedsFormatting(IValueBinding? binding)
        {
            bool isFormattedType(Type? type) =>
                type != null && (type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(DateTime) || isFormattedType(Nullable.GetUnderlyingType(type)));

            bool isFormattedTypeOrObj(Type? type) => type == typeof(object) || isFormattedType(type);

            return isFormattedType(binding?.ResultType) && isFormattedTypeOrObj(binding?.GetProperty<ExpectedTypeBindingProperty>(ErrorHandlingMode.ReturnNull)?.Type);
        }

        protected override bool RendersHtmlTag => RenderSpanElement;

        public bool IsFormattingRequired =>
            !string.IsNullOrEmpty(FormatString) ||
#pragma warning disable
            ValueType != FormatValueType.Text ||
#pragma warning restore
            NeedsFormatting(GetValueBinding(TextProperty));

        private new struct RenderState
        {
            public object? Text;
            public bool RenderSpanElement;
            public bool HasFormattingStuff;
            public DotvvmControl.RenderState BaseState;
            public HtmlGenericControl.RenderState HtmlState;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TouchProperty(DotvvmProperty prop, object? value, ref RenderState r)
        {
            if (prop == TextProperty)
                r.Text = value;
            else if (prop == RenderSpanElementProperty)
                r.RenderSpanElement = (bool)EvalPropertyValue(RenderSpanElementProperty, value)!;
#pragma warning disable CS0618
            else if (prop == FormatStringProperty || prop == ValueTypeProperty)
#pragma warning restore CS0618
                r.HasFormattingStuff = true;
            else if (base.TouchProperty(prop, value, ref r.HtmlState)) { }
            else if (DotvvmControl.TouchProperty(prop, value, ref r.BaseState)) { }
            else return false;
            return true;
        }

        protected override void RenderControl(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RenderState r = default;
            r.RenderSpanElement = true;
            foreach (var (prop, value) in properties)
                TouchProperty(prop, value, ref r);

            r.HtmlState.RendersHtmlTag = r.RenderSpanElement;

            if (base.RenderBeforeControl(in r.BaseState, writer, context))
                return;

            base.AddAttributesCore(writer, ref r.HtmlState);

            var textBinding = r.Text as IValueBinding;
            var isFormattingRequired = (r.HasFormattingStuff || textBinding != null) && this.IsFormattingRequired;

            // render Knockout data-bind
            string? expression = null;
            if (textBinding != null && !r.HtmlState.RenderOnServer(this))
            {
                expression = textBinding.GetKnockoutBindingExpression(this);
                if (isFormattingRequired)
                {
                    // almost always the Literal will be rendered before script resources are, so requesting the resource in render should be safe. In case it's not, user can always add it manually (the error message should be quite clear).
                    context.ResourceManager.AddCurrentCultureGlobalizationResource();

                    expression = "dotvvm.globalize.formatString(" + JsonConvert.ToString(FormatString) + ", " + expression + ")";
                }

                if (r.RenderSpanElement)
                {
                    writer.AddKnockoutDataBind("text", expression);
                }
            }

            // render start tag
            if (r.RenderSpanElement)
            {
                writer.RenderBeginTag(TagName!);
            }
            else if (expression != null)
            {
                writer.WriteKnockoutDataBindComment("text", expression);
            }

            if (expression == null)
            {
                string textToDisplay;
                if (isFormattingRequired)
                {
                    var formatString = FormatString;
                    if (string.IsNullOrEmpty(formatString))
                    {
                        formatString = "G";
                    }
                    textToDisplay = string.Format("{0:" + formatString + "}", EvalPropertyValue(TextProperty, r.Text));
                }
                else
                {
                    textToDisplay = EvalPropertyValue(TextProperty, r.Text)?.ToString() ?? "";
                }
                writer.WriteText(textToDisplay);
            }

            // render end tag
            if (r.RenderSpanElement)
            {
                writer.RenderEndTag();
            }
            else if (expression != null)
            {
                writer.WriteKnockoutDataBindEndComment();
            }

            base.RenderAfterControl(in r.BaseState, writer);
        }
    }
}
