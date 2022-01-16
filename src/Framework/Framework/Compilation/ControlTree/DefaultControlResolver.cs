using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private static object dotvvmLocker = new object();
        private static bool isDotvvmInitialized = false;


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

        internal static Task InvokeStaticConstructorsOnDotvvmControls()
        {
            if (isDotvvmInitialized) return Task.CompletedTask;
            return Task.Run(() => {
                if (isDotvvmInitialized) return;
                lock(dotvvmLocker) {
                    if (isDotvvmInitialized) return;
                    InvokeStaticConstructorsOnAllControls(typeof(DotvvmControl).Assembly);
                }
            });
        }

        /// <summary>
        /// Invokes the static constructors on all controls to register all <see cref="DotvvmProperty"/>.
        /// </summary>
        private void InvokeStaticConstructorsOnAllControls()
        {
            var dotvvmAssembly = typeof(DotvvmControl).Assembly.GetName().Name!;
            var dotvvmInitTask = InvokeStaticConstructorsOnDotvvmControls();

            if (configuration.ExperimentalFeatures.ExplicitAssemblyLoading.Enabled)
            {
                // use only explicitly specified assemblies from configuration
                // and do not call GetTypeInfo to prevent unnecessary dependent assemblies from loading
                var assemblies = compiledAssemblyCache.GetAllAssemblies()
                    .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                    .Distinct();

                Parallel.ForEach(assemblies, a => {
                    InvokeStaticConstructorsOnAllControls(a);
                });
            }
            else
            {
                var assemblies = GetAllRelevantAssemblies(dotvvmAssembly);
                Parallel.ForEach(assemblies, a => {
                    InvokeStaticConstructorsOnAllControls(a);
                });
            }
            dotvvmInitTask.Wait();
        }

        private static void InvokeStaticConstructorsOnAllControls(Assembly assembly)
        {
            foreach (var c in assembly.GetLoadableTypes())
            {
                if (!c.IsClass || c.ContainsGenericParameters)
                    continue;

                if (c.IsDefined(typeof(ContainsDotvvmPropertiesAttribute), true))
                    InitType(c);
            }
        }
        

        private static void InitType(Type type)
        {
            if (type.BaseType != null)
                InitType(type.BaseType);

            RuntimeHelpers.RunClassConstructor(type.TypeHandle);

            RegisterCompositeControlProperties(type);
            RegisterCapabilitiesFromInterfaces(type);
        }

        private static void RegisterCompositeControlProperties(Type type)
        {
            if (!type.IsAbstract && typeof(CompositeControl).IsAssignableFrom(type))
            {
                CompositeControl.RegisterProperties(type);
            }
        }

        private static void RegisterCapabilitiesFromInterfaces(Type type)
        {
            foreach (var capability in type.GetInterfaces())
            {
                if (capability.IsGenericType && capability.GetGenericTypeDefinition() == typeof(IObjectWithCapability<>))
                {
                    var capabilityType = capability.GetGenericArguments()[0];
                    // defined in generic type and contains generic arguments
                    // it will be probably registered in a derived control
                    if (capabilityType.ContainsGenericParameters)
                        continue;

                    if (DotvvmCapabilityProperty.GetCapabilities(type).Any(c => c.PropertyType == capabilityType))
                        continue;

                    DotvvmCapabilityProperty.RegisterCapability(type, capabilityType, capabilityAttributeProvider: new CustomAttributesProvider());
                }
            }
        }

        private IEnumerable<Assembly> GetAllRelevantAssemblies(string dotvvmAssembly)
        {

#if DotNetCore
            var assemblies = compiledAssemblyCache.GetAllAssemblies()
                   .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly));
#else

            var loadedAssemblies = compiledAssemblyCache.GetAllAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly));

            var visitedAssemblies = new HashSet<string>();

            // ReflectionUtils.GetAllAssemblies() in netframework returns only assemblies which have already been loaded into
            // the current AppDomain, to return all assemblies we traverse recursively all referenced Assemblies
            var assemblies = loadedAssemblies
                .SelectRecursively(a => a.GetReferencedAssemblies().Where(an => visitedAssemblies.Add(an.FullName)).Select(an => Assembly.Load(an)))
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                .Distinct();
#endif
            return assemblies;
        }

        /// <summary>
        /// After all DotvvmProperties have been registered, those marked with PropertyAliasAttribute can be resolved.
        /// </summary>
        private void ResolveAllPropertyAliases()
        {
            foreach (var alias in DotvvmProperty.AllAliases)
                DotvvmPropertyAlias.Resolve(alias);
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

        protected override IPropertyDescriptor? FindGlobalPropertyOrGroup(string name, MappingMode requiredMode)
        {
            // try to find property
            var property = DotvvmProperty.ResolveProperty(name, caseSensitive: false);
            if (property != null)
            {
                return property;
            }

            // try to find property group
            return DotvvmPropertyGroup.ResolvePropertyGroup(name, caseSensitive: false, requiredMode);
        }
    }
}
