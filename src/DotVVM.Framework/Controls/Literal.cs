using System;
using System.Linq;
using System.Net;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
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
    public class Literal : HtmlGenericControl
    {

        /// <summary>
        /// Gets or sets the text displayed in the control.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<object, Literal>(t => t.Text, "");


        /// <summary>
        /// Gets or sets the format string that will be applied to numeric or date-time values.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }
        public static readonly DotvvmProperty FormatStringProperty =
            DotvvmProperty.Register<string, Literal>(c => c.FormatString, "");

        /// <summary>
        /// Gets or sets whether the literal should render the wrapper span HTML element.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public bool RenderSpanElement
        {
            get { return (bool)GetValue(RenderSpanElementProperty); }
            set { SetValue(RenderSpanElementProperty, value); }
        }
        public static readonly DotvvmProperty RenderSpanElementProperty =
            DotvvmProperty.Register<bool, Literal>(t => t.RenderSpanElement, true);

        private bool renderAsKnockoutBinding;
        private string knockoutBindingExpression;

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(bool allowImplicitLifecycleRequirements = true) : base("span")
        {
            if (allowImplicitLifecycleRequirements && GetType() == typeof(Literal)) LifecycleRequirements = ControlLifecycleRequirements.PreRender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        public Literal(string text, bool allowImplicitLifecycleRequirements = true) : base("span", false)
        {
            Text = text;
            RenderSpanElement = false;
            if (allowImplicitLifecycleRequirements && GetType() == typeof(Literal)) LifecycleRequirements = ControlLifecycleRequirements.None;
        }

        public static bool NeedsFormatting(IValueBinding binding) => binding != null && (binding.ResultType == typeof(DateTime) || ReflectionUtils.IsNumericType(binding.ResultType));

        protected override bool RendersHtmlTag => RenderSpanElement;

        public bool IsFormattingRequired => !string.IsNullOrEmpty(FormatString) || NeedsFormatting(GetValueBinding(TextProperty));

        protected internal override void OnPreRender(Hosting.IDotvvmRequestContext context)
        {
            base.OnPreRender(context);

            if (IsFormattingRequired)
            {
                context.ResourceManager.AddCurrentCultureGlobalizationResource();
            }
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.AddAttributesToRender(writer, context);

            // render Knockout data-bind
            renderAsKnockoutBinding = HasBinding<IValueBinding>(TextProperty) && !RenderOnServer;
            if (renderAsKnockoutBinding)
            {
                var expression = GetValueBinding(TextProperty).GetKnockoutBindingExpression(this);
                if (IsFormattingRequired)
                {
                    expression = "dotvvm.globalize.formatString(" + JsonConvert.SerializeObject(FormatString) + ", " + expression + ")";
                }
                knockoutBindingExpression = expression;

                if (RenderSpanElement)
                {
                    writer.AddKnockoutDataBind("text", expression);
                }
            }
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderSpanElement)
            {
                base.RenderBeginTag(writer, context);
            }
            else if (renderAsKnockoutBinding)
            {
                writer.WriteKnockoutDataBindComment("text", knockoutBindingExpression);
            }
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!renderAsKnockoutBinding)
            {
                var textToDisplay = "";
                if (IsFormattingRequired)
                {
                    var formatString = FormatString;
                    if (string.IsNullOrEmpty(formatString))
                    {
                        formatString = "G";
                    }
                    textToDisplay = string.Format("{0:" + formatString + "}", GetValue(TextProperty));
                }
                else
                {
                    textToDisplay = GetValue(TextProperty)?.ToString() ?? "";
                }

                writer.WriteText(textToDisplay);
            }
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (RenderSpanElement)
            {
                base.RenderEndTag(writer, context);
            }
            else if (renderAsKnockoutBinding)
            {
                writer.WriteKnockoutDataBindEndComment();
            }
        }
    }
}