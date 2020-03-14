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
        private readonly IControlBuilderFactory controlBuilderFactory;

        private static object locker = new object();
        private static bool isInitialized = false;


        public DefaultControlResolver(DotvvmConfiguration configuration, IControlBuilderFactory controlBuilderFactory) : base(configuration.Markup)
        {
            this.controlBuilderFactory = controlBuilderFactory;

            if (!isInitialized)
            {
                lock (locker)
                {
                    if (!isInitialized)
                    {
                        var startupTracer = configuration.ServiceProvider.GetService<IStartupTracer>();

                        startupTracer?.TraceEvent(StartupTracingConstants.InvokeAllStaticConstructorsStarted);
                        InvokeStaticConstructorsOnAllControls();
                        startupTracer?.TraceEvent(StartupTracingConstants.InvokeAllStaticConstructorsFinished);

                        isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the static constructors on all controls to register all <see cref="DotvvmProperty"/>.
        /// </summary>
        private static void InvokeStaticConstructorsOnAllControls()
        {
            // PERF: too many allocations - type.GetCustomAttribute<T> does ~220k allocs -> 4MB, get all types allocates additional 1.5MB
            var dotvvmAssembly = typeof(DotvvmControl).GetTypeInfo().Assembly.GetName().Name;

#if DotNetCore
            var allTypes = ReflectionUtils.GetAllAssemblies()
               .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
               .Concat(new[] { typeof(DotvvmControl).GetTypeInfo().Assembly })
               .SelectMany(a => a.GetLoadableTypes()).Where(t => t.GetTypeInfo().IsClass).ToList();
#else
 
            var loadedAssemblies = ReflectionUtils.GetAllAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly));

            var visitedAssemblies = new HashSet<string>();

            // ReflectionUtils.GetAllAssemblies() in netframework returns only assemblies which have already been loaded into
            // the current AppDomain, to return all assemblies we traverse recursively all referenced Assemblies
            var allTypes = loadedAssemblies
                .SelectRecursively(a => a.GetReferencedAssemblies().Where(an => visitedAssemblies.Add(an.FullName)).Select(an => Assembly.Load(an)))
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                .Distinct()
                .Concat(new[] { typeof(DotvvmControl).GetTypeInfo().Assembly })
                .SelectMany(a => a.GetLoadableTypes()).Where(t => t.GetTypeInfo().IsClass);
#endif

            foreach (var type in allTypes)
            {
                if (type.GetTypeInfo().GetCustomAttribute<ContainsDotvvmPropertiesAttribute>(true) != null)
                {
                    var tt = type;
                    do
                    {
                        RuntimeHelpers.RunClassConstructor(tt.TypeHandle);
                        tt = tt.GetTypeInfo().BaseType;
                    }
                    while (tt != null && tt.GetTypeInfo().IsGenericType);
                }
            }
        }

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        public override IControlResolverMetadata ResolveControl(ITypeDescriptor controlType)
        {
            var type = ((ResolvedTypeDescriptor) controlType).Type;
            return ResolveControl(new ControlType(type));
        }


        /// <summary>
        /// Finds the compiled control.
        /// </summary>
        protected override IControlType FindCompiledControl(string tagName, string namespaceName, string assemblyName)
        {
            var type = ReflectionUtils.FindType(namespaceName + "." + tagName + ", " + assemblyName, ignoreCase: true);
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
            return new ControlResolverMetadata((ControlType) type);
        }

        protected override IPropertyDescriptor FindGlobalProperty(string name)
        {
            return DotvvmProperty.ResolveProperty(name, caseSensitive: false);
        }
    }
}
