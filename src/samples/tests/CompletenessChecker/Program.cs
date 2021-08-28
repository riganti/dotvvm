using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DotVVM.Testing.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace DotVVM.Samples.Tests.CompletenessChecker
{
    internal static class Program
    {
        // this utility compares the UI tests and Selenium tests and reports samples which do not have tests
        private static void Main(string[] args)
        {
            var testAssemblies = new[] { "DotVVM.Samples.Tests.New" };
            var testMethodAttributes = new[] { typeof(TestMethodAttribute), typeof(FactAttribute), typeof(TheoryAttribute) };

            // get a list of tests
            var testMethods = testAssemblies
                .Select(Assembly.Load)
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass)
                .SelectMany(t => t.GetMethods())
                .Where(m => testMethodAttributes.Any(attrType => m.GetCustomAttributes(attrType, true).Any()));

            // get samples used by any of the tests
            var samplesUsedByTests = testMethods
                .SelectMany(m => new[] { m.Name }.Concat(m.GetCustomAttributes<SampleReferenceAttribute>().Select(a => FixSampleName(a.SampleName))))
                .Distinct()
                .ToList();

            // get a list of samples from the web app
            var allSamples = typeof(SamplesRouteUrls).GetFields(BindingFlags.Public | BindingFlags.Static)
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

        private static Dictionary<string, string> categoryDict = new Dictionary<string, string> {
                { "ComplexSamples", "Complex" },
                { "ControlSamples", "Control" },
                { "FeatureSamples", "Feature" },
                { "Errors", "Error" }
            };

        private static string FixSampleName(string sampleName)
        {
            var parts = sampleName.Split('_');

            if (categoryDict.TryGetValue(parts[0], out var newCategoryName))
            {
                parts[0] = newCategoryName;
            }

            return string.Join("_", parts);
        }
    }
}
