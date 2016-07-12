//using System;

//namespace DotVVM.Samples.BasicSamples
//{
//    public class SwitchMiddlewareCase
//    {

//        public Func<HttpContext, bool> Condition { get; set; }

//        public Func<OwinMiddleware, OwinMiddleware> MiddlewareFactory { get; set; }


//        public SwitchMiddlewareCase(Func<HttpContext, bool> condition, Func<OwinMiddleware, OwinMiddleware> middlewareFactory)
//        {
//            Condition = condition;
//            MiddlewareFactory = middlewareFactory;
//        }

//    }
//}