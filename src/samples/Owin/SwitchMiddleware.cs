using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace DotVVM.Samples.BasicSamples
{
    public class SwitchMiddleware : OwinMiddleware
    {
        private List<Func<IOwinContext, bool>> conditions;
        private List<OwinMiddleware> middlewares;

        public SwitchMiddleware(OwinMiddleware next, List<SwitchMiddlewareCase> options) : base(next)
        {
            conditions = options.Select(o => o.Condition).ToList();
            middlewares = options.Select(o => o.MiddlewareFactory(next)).ToList();
        }

        public override Task Invoke(IOwinContext context)
        {
            for (var i = 0; i < conditions.Count; i++)
            {
                if (conditions[i](context))
                {
                    return middlewares[i].Invoke(context);
                }
            }
            return Next.Invoke(context);
        }
    }
}
