using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Compilation.Static;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    public class DependencyContextCompilerExecutor : ICompilerExecutor
    {
        private readonly Assembly assembly;
        private readonly DefaultCompilerExecutor inner;

        public DependencyContextCompilerExecutor(Assembly assembly)
        {
            this.assembly = assembly;
            inner = new(assembly);
        }

        public bool ExecuteCompile(CompilerArgs args)
        {
            ReplaceDefaultDependencyContext(assembly);
            return inner.ExecuteCompile(args);
        }

        private static void ReplaceDefaultDependencyContext(Assembly projectAssembly)
        {
#if NET472
            return;
#else
            var projectContext = Microsoft.Extensions.DependencyModel.DependencyContext.Load(projectAssembly);
            var mergedContext = Microsoft.Extensions.DependencyModel.DependencyContext.Default.Merge(projectContext);
            var fields = typeof(Microsoft.Extensions.DependencyModel.DependencyContext)
                 .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)!;
            foreach (var field in fields)
            {
                field.SetValue(
                    Microsoft.Extensions.DependencyModel.DependencyContext.Default,
                    field.GetValue(mergedContext));
            }
#endif
        }
    }
}
