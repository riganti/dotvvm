using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class RouteLink : HtmlGenericControl
    {

        
        [MarkupOptions(AllowBinding = false)]
        public string RouteName
        {
            get { return (string)GetValue(RouteNameProperty); }
            set { SetValue(RouteNameProperty, value); }
        }
        public static readonly RedwoodProperty RouteNameProperty =
            RedwoodProperty.Register<string, RouteLink>(c => c.RouteName);


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly RedwoodProperty TextProperty =
            RedwoodProperty.Register<string, RouteLink>(c => c.Text);


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
            var textBinding = GetValueBinding(TextProperty);
            if (textBinding == null)
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