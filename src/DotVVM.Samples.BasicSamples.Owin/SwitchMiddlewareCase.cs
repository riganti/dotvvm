using System;
using Microsoft.Owin;

namespace DotVVM.Samples.BasicSamples
{
    public class SwitchMiddlewareCase
    {

        public Func<IOwinContext, bool> Condition { get; set; }

        public Func<OwinMiddleware, OwinMiddleware> MiddlewareFactory { get; set; }


        public SwitchMiddlewareCase(Func<IOwinContext, bool> condition, Func<OwinMiddleware, OwinMiddleware> middlewareFactory)
        {
            Condition = condition;
            MiddlewareFactory = middlewareFactory;
        }
    }
}
