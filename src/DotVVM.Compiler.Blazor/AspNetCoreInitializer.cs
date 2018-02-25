using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using Microsoft.Extensions.Options;

namespace DotVVM.Compiler.Blazor
{
    class AspNetCoreInitializer
    {
        public static DotvvmConfiguration InitDotVVM(Assembly webSiteAssembly, Assembly clientSiteAssembly, string webSitePath, string outputPath, Action<IServiceCollection> registerServices)
        {
            var maybeDirectory = Path.GetDirectoryName(webSiteAssembly.Location);
            var dependencyContext = DependencyContext.Load(webSiteAssembly);
            var assemblyNames = new Lazy<List<AssemblyData>>(() => ResolveAssemblies(dependencyContext));

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                // find potential assemblies
                var assembly = assemblyNames.Value
                    .Where(a => string.Equals(a.AssemblyFileName, name.Name, StringComparison.CurrentCultureIgnoreCase))
                    .Select(a => new { AssemblyData = a, AssemblyName = AssemblyLoadContext.GetAssemblyName(a.AssemblyFullPath) })
                    .FirstOrDefault(a => a.AssemblyName.Name == name.Name && a.AssemblyName.Version.Major == name.Version.Major);

                if (assembly == null)
                {
                    if (File.Exists(Path.Combine(maybeDirectory, name.Name + ".dll")))
                        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(maybeDirectory, name.Name + ".dll"));

                    if (File.Exists(Path.Combine(maybeDirectory, name.Name + ".exe")))
                        return AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(maybeDirectory, name.Name + ".exe"));

                    return null;
                }
                else
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(assembly.AssemblyData.AssemblyFullPath);
                }
            };

            // HACK: copy assemblies to output path
            var defaultBlazorAssemblies = "netstandard.dll,mscorlib.dll,System.Core.dll,System.dll,Mono.Security.dll,System.Xml.dll,System.Data.dll,System.Numerics.dll,System.Transactions.dll,System.Diagnostics.StackTrace.dll,System.Drawing.dll,System.Globalization.Extensions.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,System.ComponentModel.Composition.dll,System.Net.Http.dll,System.Runtime.Serialization.dll,System.ServiceModel.Internals.dll,System.Runtime.Serialization.Xml.dll,System.Runtime.Serialization.Primitives.dll,System.Security.Cryptography.Algorithms.dll,System.Security.SecureString.dll,System.Web.Services.dll,System.Xml.Linq.dll,System.Xml.XPath.XDocument.dll,Microsoft.AspNetCore.Blazor.dll,Microsoft.AspNetCore.Blazor.Browser.dll,Samples.Blazor.Client.dll,DotVVM.Framework.dll,Microsoft.Extensions.Primitives.dll,System.Runtime.CompilerServices.Unsafe.dll,System.Runtime.dll,System.IO.dll,System.Runtime.Numerics.dll,System.Xml.XmlDocument.dll,System.Xml.XDocument.dll,System.Collections.dll,System.Globalization.dll,System.Threading.Tasks.dll,System.Diagnostics.Debug.dll,System.Reflection.dll,System.ComponentModel.TypeConverter.dll,System.Dynamic.Runtime.dll,System.Linq.Expressions.dll,System.Linq.dll,System.Runtime.Serialization.Formatters.dll,System.ObjectModel.dll,System.Text.RegularExpressions.dll,System.Xml.ReaderWriter.dll,System.Text.Encoding.dll,System.Runtime.Extensions.dll,System.Threading.dll,System.Reflection.Extensions.dll,System.Reflection.Primitives.dll,System.Text.Encoding.Extensions.dll,Microsoft.CSharp.dll,System.ComponentModel.Annotations.dll,System.ComponentModel.DataAnnotations.dll,DotVVM.Core.dll,System.Linq.Queryable.dll,System.Collections.Immutable.dll,Microsoft.CodeAnalysis.CSharp.dll,Microsoft.CodeAnalysis.dll,System.Reflection.Metadata.dll,System.Collections.Concurrent.dll,System.Runtime.InteropServices.dll,System.IO.FileSystem.dll,System.ValueTuple.dll,System.Diagnostics.Tools.dll,System.Resources.ResourceManager.dll,System.IO.FileSystem.Primitives.dll,System.Security.Cryptography.Primitives.dll,System.Text.Encoding.CodePages.dll,System.Threading.Tasks.Parallel.dll,System.Runtime.Loader.dll,Microsoft.Extensions.DependencyModel.dll,Microsoft.DotNet.PlatformAbstractions.dll,System.AppContext.dll,System.Runtime.InteropServices.RuntimeInformation.dll,Microsoft.AspNetCore.DataProtection.dll,Microsoft.Extensions.Logging.Abstractions.dll,Microsoft.Win32.Registry.dll,Microsoft.AspNetCore.DataProtection.Abstractions.dll,Microsoft.AspNetCore.Cryptography.Internal.dll,System.Security.Cryptography.Xml.dll,System.Security.Principal.Windows.dll,Microsoft.AspNetCore.Hosting.Abstractions.dll,Microsoft.Extensions.Configuration.Abstractions.dll,Microsoft.AspNetCore.Hosting.Server.Abstractions.dll,Microsoft.AspNetCore.Http.Features.dll,Microsoft.Extensions.FileProviders.Abstractions.dll,Microsoft.AspNetCore.Http.Abstractions.dll,System.Text.Encodings.Web.dll,Microsoft.AspNetCore.Authorization.dll,Microsoft.Extensions.WebEncoders.dll,DotVVM.Framework.Hosting.AspNetCore.dll,Microsoft.AspNetCore.Authentication.Cookies.dll,Microsoft.AspNetCore.Authentication.dll,Microsoft.AspNetCore.Authentication.Abstractions.dll,Microsoft.AspNetCore.WebUtilities.dll,System.Buffers.dll,Microsoft.Net.Http.Headers.dll,Microsoft.AspNetCore.Authentication.Core.dll,Microsoft.AspNetCore.Localization.dll,Microsoft.AspNetCore.Http.Extensions.dll".Split(',');
            foreach(AssemblyData assembly in assemblyNames.Value)
            {
                var newPath = Path.Combine(outputPath, assembly.AssemblyFileName + ".dll");
                if (!File.Exists(newPath) && !defaultBlazorAssemblies.Contains(assembly.AssemblyFileName + ".dll"))
                {
                    System.Console.WriteLine($"Copying {assembly.AssemblyFileName} to outPath.");
                    File.Copy(assembly.AssemblyFullPath, newPath);
                }
            }

            var dotvvmStartups = clientSiteAssembly.GetLoadableTypes()
                .Where(t => typeof(IDotvvmStartup).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToArray();
            if (dotvvmStartups.Length > 1) throw new Exception($"Found more than one implementation of IDotvvmStartup ({string.Join(", ", dotvvmStartups.Select(s => s.Name)) }).");
            var startup = dotvvmStartups.SingleOrDefault()?.Apply(Activator.CreateInstance).CastTo<IDotvvmStartup>();

            var configureServices =
                clientSiteAssembly.GetLoadableTypes()
                .Where(t => t.Name == "Startup")
                .Select(t => t.GetMethod("ConfigureDotvvmServices", new[] { typeof(IServiceCollection) }) ?? t.GetMethod("ConfigureServices", new[] { typeof(IServiceCollection) }))
                .Where(m => m != null)
                .Where(m => m.IsStatic || m.DeclaringType.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            if (startup == null && configureServices.Length == 0) throw new Exception($"Could not find ConfigureServices method, nor a IDotvvmStartup implementation.");

            var config = DotvvmConfiguration.CreateDefault(
                services =>
                {
                    registerServices?.Invoke(services);
                    foreach(var cs in configureServices)
                        cs.Invoke(cs.IsStatic ? null : Activator.CreateInstance(cs.DeclaringType), new object[] { services });
                });


            config.ApplicationPhysicalPath = webSitePath;
            startup.Configure(config, webSitePath);
            config.CompiledViewsAssemblies = null;

            // It should be handled by the DotvvmConfiguration.CreateDefault:

            // var configurers = config.ServiceLocator.GetServiceProvider().GetServices<IConfigureOptions<DotvvmConfiguration>>().ToArray();
            // if (startup == null && configurers.Length == 0) throw new Exception($"Could not find any IConfigureOptions<DotvvmConfiguration> nor a IDotvvmStartup implementation.");
            // foreach (var configurer in configurers)
            // {
            //     configurer.Configure(config);
            // }

            return config;
        }

        private static List<AssemblyData> ResolveAssemblies(DependencyContext dependencyContext)
        {
            return dependencyContext.CompileLibraries
                .SelectMany(l =>
                {
                    try
                    {
                        var paths = l.ResolveReferencePaths();
                        return paths.Select(p => new AssemblyData
                        {
                            Library = l,
                            AssemblyFullPath = p,
                            AssemblyFileName = Path.GetFileNameWithoutExtension(p)
                        });
                    }
                    catch (Exception)
                    {
                        return Enumerable.Empty<AssemblyData>();
                    }
                })
                .ToList();
        }
    }

    internal class AssemblyData
    {
        public CompilationLibrary Library { get; set; }
        public string AssemblyFullPath { get; set; }
        public string AssemblyFileName { get; set; }
    }
}