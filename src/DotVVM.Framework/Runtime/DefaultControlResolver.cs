using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Parser;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    /// <summary>
    /// Default DotVVM control resolver.
    /// </summary>
    public class DefaultControlResolver : IControlResolver
    {

        private readonly DotvvmConfiguration configuration;
        private readonly IControlBuilderFactory controlBuilderFactory;

        private static ConcurrentDictionary<string, ControlType> cachedTagMappings = new ConcurrentDictionary<string, ControlType>();
        private static ConcurrentDictionary<Type, ControlResolverMetadata> cachedMetadata = new ConcurrentDictionary<Type, ControlResolverMetadata>();

        private static object locker = new object();
        private static bool isInitialized = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControlResolver"/> class.
        /// </summary>
        public DefaultControlResolver(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
            this.controlBuilderFactory = configuration.ServiceLocator.GetService<IControlBuilderFactory>();

            if (!isInitialized)
            {
                lock (locker)
                {
                    if (!isInitialized)
                    {
                        InvokeStaticConstructorsOnAllControls();
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
            var dotvvmAssembly = typeof(DotvvmControl).Assembly.GetName().Name;
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == dotvvmAssembly))
                .Concat(new[] { typeof(DotvvmControl).Assembly })
                .SelectMany(a => a.GetTypes()).Where(t => t.IsClass).ToList();
            foreach (var type in allTypes)
            {
                if (type.GetCustomAttribute<ContainsDotvvmPropertiesAttribute>(true) != null)
                {
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
            }
        }


        /// <summary>
        /// Resolves the metadata for specified element.
        /// </summary>
        public virtual ControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters)
        {
            // html element has no prefix
            if (string.IsNullOrEmpty(tagPrefix))
            {
                activationParameters = new object[] { tagName };
                return ResolveControl(typeof(HtmlGenericControl));
            }

            // find cached value
            var searchKey = GetSearchKey(tagPrefix, tagName);
            activationParameters = null;
            var controlType = cachedTagMappings.GetOrAdd(searchKey, _ => FindControlType(tagPrefix, tagName));
            var metadata = ResolveControl(controlType);
            return metadata;
        }

        private static string GetSearchKey(string tagPrefix, string tagName)
        {
            return tagPrefix + ":" + tagName;
        }

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        public ControlResolverMetadata ResolveControl(ControlType controlType)
        {
            return cachedMetadata.GetOrAdd(controlType.Type, _ => BuildControlMetadata(controlType));
        }

        /// <summary>
        /// Resolves the control metadata for specified type.
        /// </summary>
        public ControlResolverMetadata ResolveControl(Type controlType)
        {
            return ResolveControl(new ControlType(controlType));
        }

        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        public virtual Type ResolveBinding(string bindingType, ref string bindingValue)
        {
            if (bindingType == Constants.ValueBinding)
            {
                return typeof(ValueBindingExpression);
            }
            else if (bindingType == Constants.CommandBinding)
            {
                return typeof(CommandBindingExpression);
            }
            //else if (bindingType == Constants.ControlStateBinding)
            //{
            //    return typeof (ControlStateBindingExpression);
            //}
            else if (bindingType == Constants.ControlPropertyBinding)
            {
                bindingValue = "_control." + bindingValue;
                return typeof(ControlPropertyBindingExpression);
            }
            else if (bindingType == Constants.ControlCommandBinding)
            {
                bindingValue = "_control." + bindingValue;
                return typeof(ControlCommandBindingExpression);
            }
            else if (bindingType == Constants.ResourceBinding)
            {
                return typeof(ResourceBindingExpression);
            }
            else if (bindingType == Constants.StaticCommandBinding)
            {
                return typeof(StaticCommandBindingExpression);
            }
            else
            {
                throw new NotSupportedException($"The binding {{{bindingType}: ... }} is unknown!");   // TODO: exception handling
            }
        }

        /// <summary>
        /// Finds the control metadata.
        /// </summary>
        protected virtual ControlType FindControlType(string tagPrefix, string tagName)
        {
            // try to match the tag prefix and tag name
            var rules = configuration.Markup.Controls.Where(r => r.IsMatch(tagPrefix, tagName));
            foreach (var rule in rules)
            {
                // validate the rule
                rule.Validate();

                if (string.IsNullOrEmpty(rule.TagName))
                {
                    // find the code only control
                    var compiledControl = FindCompiledControl(tagName, rule.Namespace, rule.Assembly);
                    if (compiledControl != null)
                    {
                        return compiledControl;
                    }
                }
                else
                {
                    // find the markup control
                    return FindMarkupControl(rule.Src);
                }
            }

            throw new Exception($"The control <{tagPrefix}:{tagName}> could not be resolved! Make sure that the tagPrefix is registered in DotvvmConfiguration.Markup.Controls collection!");
        }

        /// <summary>
        /// Finds the compiled control.
        /// </summary>
        protected virtual ControlType FindCompiledControl(string tagName, string namespaceName, string assemblyName)
        {
            var type = ReflectionUtils.FindType(namespaceName + "." + tagName + ", " + assemblyName);
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
        protected virtual ControlType FindMarkupControl(string file)
        {
            var controlBuilder = controlBuilderFactory.GetControlBuilder(file);
            return new ControlType(controlBuilder.ControlType, controlBuilder.GetType(), file);
        }

        /// <summary>
        /// Gets the control metadata.
        /// </summary>
        public virtual ControlResolverMetadata BuildControlMetadata(ControlType type)
        {
            var attribute = type.Type.GetCustomAttribute<ControlMarkupOptionsAttribute>();

            var properties = GetControlProperties(type.Type);
            var metadata = new ControlResolverMetadata()
            {
                Name = type.Type.Name,
                Namespace = type.Type.Namespace,
                HasHtmlAttributesCollection = typeof(IControlWithHtmlAttributes).IsAssignableFrom(type.Type),
                Type = type.Type,
                ControlBuilderType = type.ControlBuilderType,
                Properties = properties,
                IsContentAllowed = attribute.AllowContent,
                VirtualPath = type.VirtualPath,
                DataContextConstraint = type.DataContextRequirement,
                DefaultContentProperty = attribute.DefaultContentProperty != null ? properties[attribute.DefaultContentProperty] : null
            };
            return metadata;
        }

        /// <summary>
        /// Gets the control properties.
        /// </summary>
        protected virtual Dictionary<string, DotvvmProperty> GetControlProperties(Type controlType)
        {
            return DotvvmProperty.ResolveProperties(controlType).Concat(DotvvmProperty.GetVirtualProperties(controlType)).ToDictionary(p => p.Name, p => p);
        }

    }
}