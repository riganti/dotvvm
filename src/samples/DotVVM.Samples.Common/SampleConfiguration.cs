using System;
using System.Collections.Generic;

namespace DotVVM.Samples.BasicSamples
{
    public class SampleConfiguration
    {

        public IReadOnlyDictionary<string, string> AppSettings { get; set; }

        private SampleConfiguration(IReadOnlyDictionary<string, string> appSettings)
        {
            AppSettings = appSettings;
        }



        public static SampleConfiguration Instance { get; private set; }

        public static void Initialize(IReadOnlyDictionary<string, string> appSettings)
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("SampleConfiguration cannot be set twice!");
            }

            Instance = new SampleConfiguration(appSettings);
        }
    }
}
