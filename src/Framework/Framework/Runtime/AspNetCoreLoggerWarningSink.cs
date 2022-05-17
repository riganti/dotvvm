using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;
using Microsoft.Extensions.Logging;

namespace DotVVM.Framework.Runtime
{
    public class AspNetCoreLoggerWarningSink: IDotvvmWarningSink
    {
        private ILogger? logger;
        private readonly IDotvvmRequestContext? requestContext;

        public AspNetCoreLoggerWarningSink(
            ILogger<AspNetCoreLoggerWarningSink>? logger = null,
            IDotvvmRequestContext? requestContext = null)
        {
            this.logger = logger;
            this.requestContext = requestContext;
        }

        public void RuntimeWarning(DotvvmRuntimeWarning warning)
        {
            logger?.Log(LogLevel.Warning, 0, new LogEvent(warning, requestContext), warning.RelatedException, (x, e) => x.ToString());
        }

        // custom log event implementing IEnumerable<KeyValuePair<string, object>> for Serilog's properties
        readonly struct LogEvent : IEnumerable<KeyValuePair<string, object>>
        {
            readonly DotvvmRuntimeWarning warning;
            readonly IDotvvmRequestContext? context;

            public LogEvent(DotvvmRuntimeWarning warning, IDotvvmRequestContext? context)
            {
                this.warning = warning;
                this.context = context;
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                yield return new KeyValuePair<string, object>("Message", warning.Message);
                if (warning.RelatedControl is {} control)
                {
                    yield return new KeyValuePair<string, object>("RelatedControlType", control.GetType().ToCode());
                }
                if (context is {})
                {
                    if (context.Route?.RouteName is {} route)
                        yield return new KeyValuePair<string, object>("DotvvmRouteName", route);
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            public override string ToString()
            {
                var header = context is {} ? $"DotVVM runtime error while processing {context.HttpContext.Request.Path}:" : "DotVVM runtime error:";
                return warning.Message + warning.RelatedControl?.Apply(c => "\nRelated to:\n" + c.DebugString());
            }
        }
    }
}
