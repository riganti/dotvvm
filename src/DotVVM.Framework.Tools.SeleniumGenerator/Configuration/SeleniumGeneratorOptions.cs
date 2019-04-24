using System.Collections.Generic;
using System.Reflection;
using DotVVM.Framework.Tools.SeleniumGenerator.Generators;

namespace DotVVM.Framework.Tools.SeleniumGenerator.Configuration
{
    public class SeleniumGeneratorOptions
    {
        internal HashSet<Assembly> Assemblies { get; } = new HashSet<Assembly> { typeof(SeleniumGeneratorOptions).Assembly };
        internal HashSet<ISeleniumGenerator> CustomGenerators { get; } = new HashSet<ISeleniumGenerator>();

        public void AddAssembly(Assembly assembly) => Assemblies.Add(assembly);

        public void AddCustomGenerator(ISeleniumGenerator generator) => CustomGenerators.Add(generator);
        public void AddCustomGenerators(IEnumerable<ISeleniumGenerator> generators)
        {
            foreach (var seleniumGenerator in generators)
            {
                AddCustomGenerator(seleniumGenerator);
            }
        }
    }
}