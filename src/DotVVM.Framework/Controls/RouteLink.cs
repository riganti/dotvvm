using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Hyperlink which builds the URL from route name and parameter values.
    /// </summary>
    public class RouteLink : HtmlGenericControl
    {
        private const string EnableKnockoutBinding = "dotvvmEnable";

        private bool shouldRenderText = false;

        /// <summary>
        /// Gets or sets the name of the route in the route table.
        /// </summary>
        [MarkupOptions(AllowBinding = false, Required = true)]
        public string RouteName
        {
            get { return (string)GetValue(RouteNameProperty); }
            set { SetValue(RouteNameProperty, value); }
        }
        public static readonly DotvvmProperty RouteNameProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.RouteName);

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, RouteLink>(t => t.Enabled, true);

        /// <summary>
        /// Gets or sets the suffix that will be appended to the generated URL (e.g. query string or URL fragment).
        /// </summary>
        public string UrlSuffix
        {
            get { return (string)GetValue(UrlSuffixProperty); }
            set { SetValue(UrlSuffixProperty, value); }
        }
        public static readonly DotvvmProperty UrlSuffixProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.UrlSuffix, null);


        /// <summary>
        /// Gets or sets the text of the hyperlink.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.Text, "");

        public VirtualPropertyGroupDictionary<object> Params => new VirtualPropertyGroupDictionary<object>(this, ParamsGroupDescriptor);
        public static DotvvmPropertyGroup ParamsGroupDescriptor =
            DotvvmPropertyGroup.Register<object, RouteLink>("Param-", "Params");

        public VirtualPropertyGroupDictionary<object> QueryParameters => new VirtualPropertyGroupDictionary<object>(this, QueryParametersGroupDescriptor);
        public static DotvvmPropertyGroup QueryParametersGroupDescriptor =
            DotvvmPropertyGroup.Register<object, RouteLink>("Query-", "QueryParameters");


        public RouteLink() : base("a")
        {
        }

        protected override void AddAttributesToRender(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            RouteLinkHelpers.WriteRouteLinkHrefAttribute(this, writer, context);

            writer.AddKnockoutDataBind("text", this, TextProperty, () => {
                shouldRenderText = true;
            });

            var enabledValue = GetValueRaw(EnabledProperty);
            if (enabledValue is bool)
            {
                writer.AddKnockoutDataBind(EnableKnockoutBinding, enabledValue.ToString().ToLower());
            }
            else if (enabledValue is IValueBinding enabledBinding)
            {
                writer.AddKnockoutDataBind(EnableKnockoutBinding, enabledBinding.GetKnockoutBindingExpression(this));
            }

            WriteOnClickAttribute(writer, context);

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (!shouldRenderText)
            {
                return;
            }

            if (!HasOnlyWhiteSpaceContent())
            {
                base.RenderContents(writer, context);
            }
            else
            {
                writer.WriteText(Text);
            }
        }

        protected virtual void WriteOnClickAttribute(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            // a hack that makes the RouteLink work even in container with Events.Click. This does not solve the problem in general, but better than nothing.
            var onclickAttribute = "event.stopPropagation();";
            if ((bool)GetValue(Internal.IsSpaPageProperty) && (bool)GetValue(Internal.UseHistoryApiSpaNavigationProperty))
            {
                onclickAttribute += "return !this.hasAttribute('disabled') && dotvvm.handleSpaNavigation(this);";
            }
            else
            {
                onclickAttribute += "return !this.hasAttribute('disabled');";
            }
            writer.AddAttribute("onclick", onclickAttribute);
        }
    }
}
