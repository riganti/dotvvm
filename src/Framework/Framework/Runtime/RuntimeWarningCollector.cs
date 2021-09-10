using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using Microsoft.Extensions.Options;

namespace DotVVM.Framework.Runtime
{

    /// <summary>
    /// A request-scoped service used to collect warnings for debugging. By default these warnings are displayed in browser console and pushed to ASP.NET Core logging (thus probably displayed in the web server console).
    ///
    /// Although this is request scoped, it is thread-safe.
    /// </summary>
    public class RuntimeWarningCollector
    {
        IDotvvmWarningSink[] sinks;
        ConcurrentStack<DotvvmRuntimeWarning> warnings = new ConcurrentStack<DotvvmRuntimeWarning>();
        public bool Enabled { get; }

        public RuntimeWarningCollector(IEnumerable<IDotvvmWarningSink> sinks, DotvvmConfiguration config)
        {
            this.Enabled = config.Debug;
            this.sinks = sinks.ToArray();
        }

        public void Warn(DotvvmRuntimeWarning warning)
        {
            if (!Enabled) return;

            this.warnings.Push(warning);
            foreach (var s in sinks)
                s.RuntimeWarning(warning);
        }


        public List<DotvvmRuntimeWarning> GetWarnings() => warnings.ToList();

        // public void Warn(string message, Exception relatedException = null, DotvvmBindableObject relatedControl = null) =>
        //     Warn(message, relatedException, relatedControl);
    }
}
