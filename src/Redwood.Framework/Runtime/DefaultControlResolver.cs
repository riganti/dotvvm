using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Redwood.Framework.Binding;
using Redwood.Framework.Configuration;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Runtime
{
    /// <summary>
    /// Default Redwood control resolver.
    /// </summary>
    public class DefaultControlResolver : IControlResolver
    {

        private readonly RedwoodConfiguration configuration;

        private ConcurrentDictionary<string, ControlResolverMetadata> cachedControlMetadata = new ConcurrentDictionary<string, ControlResolverMetadata>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultControlResolver"/> class.
        /// </summary>
        public DefaultControlResolver(RedwoodConfiguration configuration)
        {
            this.configuration = configuration;
            cachedControlMetadata[string.Empty] = BuildControlMetadata(typeof(HtmlGenericControl));
        }


        /// <summary>
        /// Resolves the type of a control.
        /// </summary>
        public ControlResolverMetadata ResolveControl(string tagPrefix, string tagName, out object[] activationParameters)
        {
            if (string.IsNullOrEmpty(tagPrefix))
            {
                activationParameters = new object[] { tagName };
                return cachedControlMetadata[string.Empty];
            }

            activationParameters = null;
            return FindControlMetadata(tagPrefix, tagName);
        }
        
        /// <summary>
        /// Resolves the binding type.
        /// </summary>
        public Type ResolveBinding(string bindingType)
        {
            if (bindingType == "value")
            {
                return typeof (ValueBindingExpression);
            }
            else if (bindingType == "command")
            {
                return typeof (CommandBindingExpression);
            }
            else if (bindingType == "controlState")
            {
                return typeof (ControlStateBindingExpression);
            }
            else
            {
                throw new NotSupportedException("Unknown binding type!");   // TODO: exception handling
            }
        }

        /// <summary>
        /// Finds the control metadata.
        /// </summary>
        private ControlResolverMetadata FindControlMetadata(string tagPrefix, string tagName)
        {
            // try to find cached control metadata in specified namespace
            var namespaces = configuration.Markup.Controls.Where(c => c.TagPrefix == tagPrefix).SelectMany(c => c.Namespaces).ToList();
            foreach (var type in namespaces.Select(c => c + "." + tagName))
            {
                // get control metadata
                ControlResolverMetadata metadata;
                if (cachedControlMetadata.TryGetValue(type, out metadata))
                {
                    return metadata;
                }
            }

            // try to find the type in all namespaces and build metadata
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var type in namespaces.Select(c => c + "." + tagName))
            {
                var foundTypes = assemblies
                    .Select(a => a.GetType(type))
                    .Where(a => a != null)
                    .ToList();

                if (foundTypes.Count == 1)
                {
                    // build and store the metadata
                    var metadata = BuildControlMetadata(foundTypes[0]);
                    cachedControlMetadata[tagPrefix + "." + tagName] = metadata;
                    return metadata;
                }
                if (foundTypes.Count > 1)
                {
                    throw new Exception(string.Format(Resources.Controls.ControlResolver_DuplicateControlRegistration, tagPrefix, tagName));
                }
            }
            throw new Exception(string.Format(Resources.Controls.ControlResolver_ControlNotFound, tagPrefix, tagName));
        }

        /// <summary>
        /// Gets the control metadata.
        /// </summary>
        private ControlResolverMetadata BuildControlMetadata(Type controlType)
        {
            var metadata = new ControlResolverMetadata()
            {
                Name = controlType.Name,
                Namespace = controlType.Namespace,
                HasHtmlAttributesCollection = typeof(IControlWithHtmlAttributes).IsAssignableFrom(controlType),
                Type = controlType,
                Properties = controlType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new ControlResolverPropertyMetadata()
                    {
                        Options = p.GetCustomAttribute<MarkupOptionsAttribute>() ?? new MarkupOptionsAttribute()
                            {
                                AllowBinding = true,
                                AllowHardCodedValue = true,
                                MappingMode = MappingMode.Attribute,
                                Name = p.Name
                            },
                        PropertyInfo = p
                    })
                    .Select(p =>
                    {
                        if (p.Options.Name == null)
                        {
                            p.Options.Name = p.PropertyInfo.Name;
                        }
                        return p;
                    })
                    .ToDictionary(p => p.Options.Name, p => p)
            };
            return metadata;
        }
    }
}