//using Microsoft.AspNetCore.Http;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace DotVVM.Samples.BasicSamples
//{
//    public class SwitchMiddleware
//    {
//        private List<Func<HttpContext, bool>> conditions;
//        private List<RequestDelegate> middlewares;
//		private readonly RequestDelegate next;

//		public SwitchMiddleware(RequestDelegate next, List<SwitchMiddlewareCase> options)
//        {
//			this.next = next;
//            conditions = options.Select(o => o.Condition).ToList();
//            middlewares = options.Select(o => o.MiddlewareFactory(next)).ToList();
//        }

//        public Task Invoke(HttpContext context)
//        {
//            for (var i = 0; i < conditions.Count; i++)
//            {
//                if (conditions[i](context))
//                {
//                    return middlewares[i].Invoke(context);
//                }
//            }
//            return next(context);
//        }
//    }
//}