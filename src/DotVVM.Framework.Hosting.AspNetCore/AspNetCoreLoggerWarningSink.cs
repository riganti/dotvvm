using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Runtime;
using Microsoft.Extensions.Logging;

namespace DotVVM.Framework.Hosting.AspNetCore
{
    public class AspNetCoreLoggerWarningSink: IDotvvmWarningSink
    {
        private ILogger logger;

        public AspNetCoreLoggerWarningSink(ILogger<AspNetCoreLoggerWarningSink> logger = null)
        {
            this.logger = logger;
        }

        public void RuntimeWarning(DotvvmRuntimeWarning warning)
        {
            logger?.Log(LogLevel.Warning, "{0}", warning);
        }
    }
}
