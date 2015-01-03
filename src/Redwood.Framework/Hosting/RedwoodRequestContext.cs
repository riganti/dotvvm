using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Routing;

namespace Redwood.Framework.Hosting
{
    public class RedwoodRequestContext
    {


        public IOwinContext OwinContext { get; internal set; }
        
        public IRedwoodPresenter Presenter { get; internal set; }

        public RedwoodConfiguration Configuration { get; internal set; }

        public RouteBase Route { get; internal set; }

        public bool IsPostBack { get; internal set; }

        public IDictionary<string, object> Parameters { get; set; }

        public ResourceManager ResourceManager { get; set; }

        public IReadableStringCollection Query
        {
            get
            {
                return OwinContext.Request.Query;
            }
        }
    }
}
