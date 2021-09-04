using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Filters;

namespace DotVVM.Samples.BasicSamples.Api.Owin
{
    public class NoCacheAttribute : ActionFilterAttribute 
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true
            };
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
