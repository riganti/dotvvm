using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Tracing;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class DefaultControlResolver : ControlResolverBase
    {
        private readonly DotvvmConfiguration configuration;
        private readonly IControlBuilderFactory controlBuilderFactory;
        private readonly CompiledAssemblyCache compiledAssemblyCache;

        private static object locker = new object();
        private static bool isInitialized = false;


        public DefaultControlResolver(DotvvmConfiguration configuration, IControlBuilderFactory controlBuilderFactory, CompiledAssemblyCache compiledAssemblyCache) : base(configuration.Markup)
        {
            this.configuration = configuration;
            this.controlBuilderFactory = controlBuilderFactory;
            this.compiledAssemblyCache = compiledAssemblyCache;

            if (!isInitialized)
            {
                lock (locker)
                {
                    if (!isInitialized)
                    {
                        var startupTracer = configuration.ServiceProvider.GetService<IStartupTracer>();

                        startupTracer?.TraceEvent(StartupTracingConstants.InvokeAllStaticConstructorsStarted);
                        InvokeStaticConstructorsOnAllControls();
                        ResolveAllPropertyAliases();
                        startupTracer?.TraceEvent(StartupTracingConstants.InvokeAllStaticConstructorsFinished);

                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the static constructors on all controls to register all <see cref="DotvvmProperty"/>.
        /// </summary>
        private void InvokeStaticConstructorsOnAllControls()
        {
            var dotvvmAssembly = typeof(DotvvmControl).Assembly.GetName().Name;

            if (configuration.ExperimentalFeatures.ExplicitAssemblyLoading.Enabled)
            {
                // use only explicitly specified assemblies from configuration
                // and do not call GetTypeInfo to prevent unnecessary dependent assemblies from loading
                var allTypes = compiledAssemblyCache.GetAllAssemblies()
                    .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                    .Concat(new[] { typeof(DotvvmControl).Assembly })
                    .Distinct()
                    .SelectMany(a => a.GetLoadableTypes()).Where(t => t.IsClass);

                foreach (var type in allTypes)
                {
                    if (type.GetCustomAttribute<ContainsDotvvmPropertiesAttribute>(true) != null)
                    {
                        var tt = type;
                        do
                        {
                            RegisterCompositeControlProperties(tt);
                            RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
                            tt = tt.BaseType;
                        }
                        while (tt != null);
                    }
                }
            }
            else
            {
                // keep the old way as it was
                // PERF: too many allocations - type.GetCustomAttribute<T> does ~220k allocs -> 4MB, get all types allocates additional 1.5MB
                var allTypes = GetAllLoadableTypes(dotvvmAssembly);
                foreach (var type in allTypes)
                {
                    if (type.GetCustomAttribute<ContainsDotvvmPropertiesAttribute>(true) != null)
                    {
                        var tt = type;
                        do
                        {
                            RegisterCompositeControlProperties(tt);
                            RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
                            tt = tt.BaseType;
                        }
                        while (tt != null);
                    }
                }
            }
        }

        private static void RegisterCompositeControlProperties(Type type)
        {
            if (type is object && !type.IsAbstract && typeof(CompositeControl).IsAssignableFrom(type))
            {
                CompositeControl.RegisterProperties(type);
            }
        }

        private IEnumerable<Type> GetAllLoadableTypes(string dotvvmAssembly)
        {

#if DotNetCore
            var allTypes = compiledAssemblyCache.GetAllAssemblies()
                   .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                   .Concat(new[] { typeof(DotvvmControl).Assembly })
                   .SelectMany(a => a.GetLoadableTypes()).Where(t => t.IsClass).ToList();
#else

            var loadedAssemblies = compiledAssemblyCache.GetAllAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly));

            var visitedAssemblies = new HashSet<string>();

            // ReflectionUtils.GetAllAssemblies() in netframework returns only assemblies which have already been loaded into
            // the current AppDomain, to return all assemblies we traverse recursively all referenced Assemblies
            var allTypes = loadedAssemblies
                .SelectRecursively(a => a.GetReferencedAssemblies().Where(an => visitedAssemblies.Add(an.FullName)).Select(an => Assembly.Load(an)))
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                .Distinct()
                .Concat(new[] { typeof(DotvvmControl).Assembly })
                .SelectMany(a => a.GetLoadableTypes()).Where(t => t.IsClass);
#endif
            return allTypes;
        }

        /// <summary>
        /// After all DotvvmProperties have been registered, those marked with PropertyAliasAttribute can be resolved.
        /// </summary>
        private void ResolveAllPropertyAliases()
        {
            foreach (var alias in DotvvmProperty.GetRegisteredAliases()) {
                DotvvmPropertyAlias.Resolve(alias.Value);
            }
        }

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        public override IControlResolverMetadata ResolveControl(ITypeDescriptor controlType)
        {
            var type = ((ResolvedTypeDescriptor)controlType).Type;
            return ResolveControl(new ControlType(type));
        }


        /// <summary>
        /// Finds the compiled control.
        /// </summary>
        protected override IControlType? FindCompiledControl(string tagName, string namespaceName, string assemblyName)
        {
            var type = compiledAssemblyCache.FindType(namespaceName + "." + tagName + ", " + assemblyName, ignoreCase: true);
            if (type == null)
            {
                // the control was not found
                return null;
            }

            return new ControlType(type);
        }


        /// <summary>
        /// Finds the markup control.
        /// </summary>
        protected override IControlType FindMarkupControl(string file)
        {
            var (descriptor, controlBuilder) = controlBuilderFactory.GetControlBuilder(file);
            return new ControlType(descriptor.ControlType, file, descriptor.DataContextType);
        }

        /// <summary>
        /// Gets the control metadata.
        /// </summary>
        public override IControlResolverMetadata BuildControlMetadata(IControlType type)
        {
            return new ControlResolverMetadata((ControlType)type);
        }

        protected override IPropertyDescriptor? FindGlobalPropertyOrGroup(string name)
        {
            // try to find property
            var property = DotvvmProperty.ResolveProperty(name, caseSensitive: false);
            if (property != null)
            {
                return property;
            }

            // try to find property group
            return DotvvmPropertyGroup.ResolvePropertyGroup(name, caseSensitive: false);
        }
    }
}
