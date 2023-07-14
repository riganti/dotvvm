using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace DotVVM.Adapters.WebForms.Tests
{
    public static class WebFormsRouteTableInit
    {

        static WebFormsRouteTableInit()
        {
            RouteTable.Routes.Add("NoParams", new Route("", new EmptyHandler()));
            RouteTable.Routes.Add("SingleParam", new Route("page/{Index}", new EmptyHandler()));
            RouteTable.Routes.Add("MultipleOptionalParams", new Route("catalog/{Tag}/{SubTag}", new EmptyHandler()) { Defaults = new RouteValueDictionary(new { Tag = "xx", SubTag = "yy" })});
        }

        public static void EnsureInitialized()
        {
        }

    }

    public class EmptyHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext) => throw new NotImplementedException();
    }
}
