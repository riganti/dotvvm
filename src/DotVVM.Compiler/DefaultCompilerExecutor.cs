using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Static;
using DotVVM.Framework.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Compiler
{
    public class DefaultCompilerExecutor : ICompilerExecutor
    {
        private readonly Assembly assembly;

        public DefaultCompilerExecutor(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public bool ExecuteCompile(CompilerArgs args)
        {
            if (args.IsListProperties)
            {
                ListProperties();
                return true;
            }

            var projectDirPath = args.ProjectDir?.FullName ?? Directory.GetCurrentDirectory();
            var reports = StaticViewCompiler.CompileAll(assembly, projectDirPath);
            var logger = new DefaultCompilationReportLogger();
            using var err = Console.OpenStandardError();
            logger.Log(err, reports);
            return reports.Length == 0;
        }

        private void ListProperties()
        {
            var configuration = DotvvmConfiguration.CreateInternal(_ => {});
            // NB: This forces DotVVM to register all properties on all types.
            _ = configuration.ServiceProvider.GetRequiredService<IControlResolver>();
            var types = assembly.GetTypes().OrderBy(t => t.Name).ToArray();
            foreach (var type in types)
            {
                var props = DotvvmProperty.ResolveProperties(type)
                    .Where(p => p.DeclaringType == type)
                    .OrderBy(p => p.Name)
                    .ToArray();
                if (props.Length != 0)
                {
                    var columnCount = types.Max(p => p.Name.Length);
                    Console.WriteLine(type.Name);
                    foreach(var prop in props)
                    {
                        var info = new List<string>();
                        info.Add($"type={prop.PropertyType.Name}");

                        if (prop.IsBindingProperty)
                        {
                            info.Add("binding");
                        }
                        if (prop.IsVirtual)
                        {
                            info.Add("virtual");
                        }
                        if (prop.DataContextChangeAttributes.Length != 0
                            || prop.DataContextManipulationAttribute is object)
                        {
                            info.Add("changed-context");
                        }

                        Console.Write($"\t{prop.Name.PadRight(columnCount)}");
                        if (info.Count != 0)
                        {
                            Console.Write($"\t[{string.Join(", ", info)}]");
                        }
                        Console.WriteLine();
                    }
                }
            }
        }
    }
}
