using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Hyperlink which builds the URL from route name and parameter values.
    /// </summary>
    public class RouteLink : HtmlGenericControl
    {


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
        /// Gets or sets the text of the hyperlink.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DotvvmProperty TextProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.Text);


        public RouteLink() : base("a")
        {
        }


        private bool shouldRenderText = false;

        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            RouteLinkHelpers.WriteRouteLinkHrefAttribute(RouteName, this, writer, context);

            writer.AddKnockoutDataBind("text", this, TextProperty, () =>
            {
                shouldRenderText = true;
            });
            var enabledBinding = GetValueBinding(EnabledProperty);
            if (enabledBinding != null) WriteEnabledBinding(writer, enabledBinding);

            base.AddAttributesToRender(writer, context);
        }

        protected virtual void WriteEnabledBinding(IHtmlWriter writer, IValueBinding binding)
        {
            writer.AddKnockoutDataBind("dotvvmEnable", binding);
            writer.AddAttribute("onclick", "return !this.hasAttribute('disabled')");
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            if (shouldRenderText)
            {
                if (!HasOnlyWhiteSpaceContent())
                {
                    base.RenderContents(writer, context);
                }
                else
                {
                    writer.WriteText(Text);
                }
            }
        }
        
    }
}