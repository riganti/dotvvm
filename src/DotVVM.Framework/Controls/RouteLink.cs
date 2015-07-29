using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public class RouteLink : HtmlGenericControl
    {

        
        [MarkupOptions(AllowBinding = false)]
        public string RouteName
        {
            get { return (string)GetValue(RouteNameProperty); }
            set { SetValue(RouteNameProperty, value); }
        }
        public static readonly DotvvmProperty RouteNameProperty =
            DotvvmProperty.Register<string, RouteLink>(c => c.RouteName);


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


        protected override void AddAttributesToRender(IHtmlWriter writer, RenderContext context)
        {
            RouteLinkHelpers.WriteRouteLinkHrefAttribute(RouteName, this, writer, context);

            writer.AddKnockoutDataBind("text", this, TextProperty, () => { });

            base.AddAttributesToRender(writer, context);
        }

        protected override void RenderContents(IHtmlWriter writer, RenderContext context)
        {
            var textBinding = GetBinding(TextProperty);
            if (textBinding?.Javascript == null)
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    writer.WriteText(Text);
                }
                else
                {
                    base.RenderContents(writer, context);
                }
            }
        }
        
    }
}