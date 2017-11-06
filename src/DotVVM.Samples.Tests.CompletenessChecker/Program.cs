using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Samples.Tests.CompletenessChecker
{
    class Program
    {

        // this utility compares the UI tests and Selenium tests and reports samples which do not have tests
        static void Main(string[] args)
        {
            // get a list of tests
            var testMethods = typeof(SamplesRouteUrls).Assembly.GetTypes()
                .Where(t => t.IsClass)
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes<TestMethodAttribute>().Any());

            // get samples used by any of the tests
            var samplesUsedByTests = testMethods
                .SelectMany(m => new[] { m.Name }.Concat(m.GetCustomAttributes<SampleReferenceAttribute>().Select(a => FixSampleName(a.SampleName))))
                .Distinct()
                .ToList();

            // get a list of samples from the web app
            var allSamples = typeof(SamplesRouteUrls).GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.Name != "Default")
                .Select(p => FixSampleName(p.Name))
                .OrderBy(p => p)
                .ToList();

            // output the samples which are not used
            var results = allSamples.Except(samplesUsedByTests).ToList();
            foreach (var sample in results)
            {
                Console.WriteLine(sample);
            }

            if (Debugger.IsAttached)
            {
                Console.ReadKey();
            }

            Environment.Exit(results.Any() ? 1 : 0);
        }

        private static string FixSampleName(string sampleName)
        {
            var parts = sampleName.Split('_');

            if (parts[0] == "ComplexSamples")
            {
                parts[0] = "Complex";
            }
            else if (parts[0] == "ControlSamples")
            {
                parts[0] = "Control";
            }
            else if (parts[0] == "FeatureSamples")
            {
                parts[0] = "Feature";
            }
            else if (parts[0] == "Errors")
            {
                parts[0] = "Error";
            }

            return string.Join("_", parts);
        }
    }
}
