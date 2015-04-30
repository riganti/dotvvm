using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Redwood.Framework.Binding;
using Redwood.Framework.Hosting;
using Redwood.Framework.Routing;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Controls
{
    public class RouteLink : HtmlGenericControl
    {

        private const string RouteParameterPrefix = "Param-";



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
            if (!RenderOnServer)
            {
                writer.AddKnockoutDataBind("attr", "{ href: '#!/' + " + GenerateRouteLink(context) + "}");
            }
            else
            {
                writer.AddAttribute("href", GenerateRouteUrl(context));
            }

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

        private string GenerateRouteUrl(RenderContext context)
        {
            var route = GetRoute(context);
            var parameters = ComposeNewRouteParameters(context, route);

            // evaluate bindings on server
            foreach (var param in parameters.Where(p => p.Value is BindingExpression).ToList())
            {
                if (param.Value is BindingExpression)
                {
                    EnsureValidBindingType(param.Value as BindingExpression);
                    parameters[param.Key] = ((ValueBindingExpression)param.Value).Evaluate(this, null);   // TODO: see below
                }
            }

            // generate the URL
            return route.BuildUrl(parameters);
        }

        private RouteBase GetRoute(RenderContext context)
        {
            // find the route 
            var route = context.RequestContext.Configuration.RouteTable[RouteName];
            if (route == null)
            {
                throw new Exception(string.Format("Route with name '{0}' was not found!", RouteName));
            }

            return route;
        }

        private string GenerateRouteLink(RenderContext context)
        {
            var route = GetRoute(context);
            var parameters = ComposeNewRouteParameters(context, route);

            // generate the function call
            var sb = new StringBuilder();
            sb.Append("redwood.buildRouteUrl(");
            sb.Append(JsonConvert.SerializeObject(route.Url));
            sb.Append(", {");
            sb.Append(string.Join(", ", parameters.Select(TranslateRouteParameter)));
            sb.Append("})");
            return sb.ToString();
        }

        private string TranslateRouteParameter(KeyValuePair<string, object> param)
        {
            string expression = "";
            if (param.Value is BindingExpression)
            {
                EnsureValidBindingType(param.Value as BindingExpression);

                var binding = param.Value as ValueBindingExpression;
                expression = binding.TranslateToClientScript(this, null); // TODO: pass a special RedwoodProperty for dynamic parameters on this place. The function might need the value in the future.
            }
            else
            {
                expression = JsonConvert.SerializeObject(param.Value);
            }
            return JsonConvert.SerializeObject(param.Key) + ": " + expression;
        }

        private static void EnsureValidBindingType(BindingExpression binding)
        {
            if (!(binding is ValueBindingExpression))
            {
                throw new Exception("Only {value: ...} bindings are supported in <rw:RouteLink Param-xxx='' /> attributes!");
            }
        }

        private Dictionary<string, object> ComposeNewRouteParameters(RenderContext context, RouteBase route)
        {
            var parameters = new Dictionary<string, object>(route.DefaultValues);
            foreach (var param in context.RequestContext.Parameters)
            {
                parameters[param.Key] = param.Value;
            }
            foreach (var attr in GetRouteParameters())
            {
                var parameterName = attr.Key.Substring(RouteParameterPrefix.Length);
                parameters[parameterName] = attr.Value;

                // remove the attribute because we don't want to be rendered
                Attributes.Remove(attr.Key);
            }
            return parameters;
        }

        private List<KeyValuePair<string, object>> GetRouteParameters()
        {
            return Attributes.Where(a => a.Key.StartsWith(RouteParameterPrefix)).ToList();
        }
    }
}