using System.Collections.Generic;
using System.Reflection;

namespace DotVVM.Framework.Tools.SeleniumGenerator
{
    public class SeleniumGeneratorOptions
    {
        private readonly HashSet<Assembly> assemblies  = new HashSet<Assembly>();
        private readonly HashSet<ISeleniumGenerator> customGenerators = new HashSet<ISeleniumGenerator>();

        public HashSet<ISeleniumGenerator> GetCustomGenerators() => customGenerators;
        public HashSet<Assembly> GetAssemblies() => assemblies;

        public void AddAssembly(Assembly assembly) => assemblies.Add(assembly);

        public void AddCustomGenerator(ISeleniumGenerator generator) => customGenerators.Add(generator);
        public void AddCustomGenerators(IEnumerable<ISeleniumGenerator> generators)
        {
            foreach (var seleniumGenerator in generators)
            {
                AddCustomGenerator(seleniumGenerator);
            }
        }
    }
}
