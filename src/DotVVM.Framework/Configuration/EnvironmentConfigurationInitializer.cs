using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Configuration
{
    public class EnvironmentConfigurationInitializer
    {
        private static readonly ConcurrentBag<DotvvmConfiguration> initializedCofnigurations = new ConcurrentBag<DotvvmConfiguration>();
        public void Initialize(DotvvmConfiguration config, bool debug)
        {
            if (initializedCofnigurations.Any(s => object.ReferenceEquals(s, config)))
            {
                throw new InvalidOperationException("DotvvmConfiguration can be initialized only once.");
            }
            initializedCofnigurations.Add(config);
            config.Debug = debug;
        }
    }
}
