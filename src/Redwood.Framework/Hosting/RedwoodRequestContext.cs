using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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

        /// <summary>
        /// Changes the current culture of this HTTP request.
        /// </summary>
        public void ChangeCurrentCulture(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }
    }
}
