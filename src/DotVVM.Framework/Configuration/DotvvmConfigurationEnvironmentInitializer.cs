using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.Configuration
{
    public static class DotvvmConfigurationEnvironmentInitializer
    {
        private static readonly ConcurrentBag<DotvvmConfiguration> InitializedConfigurations = new ConcurrentBag<DotvvmConfiguration>();
        public static void Initialize(DotvvmConfiguration config, bool debug)
        {
            if (InitializedConfigurations.Any(s => object.ReferenceEquals(s, config)))
            {
                throw new InvalidOperationException("DotvvmConfiguration can be initialized only once.");
            }
            InitializedConfigurations.Add(config);
            config.Debug = debug;
        }
    }
}
