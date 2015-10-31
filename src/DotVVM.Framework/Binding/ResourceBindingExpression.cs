using DotVVM.Framework.Runtime.Compilation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace DotVVM.Framework.Binding
{
    [BindingCompilationRequirements(OriginalString = BindingCompilationRequirementType.StronglyRequire, Delegate = BindingCompilationRequirementType.IfPossible)]
    [BindingCompilation]
    public class ResourceBindingExpression : BindingExpression, IStaticValueBinding
    {
        private static ConcurrentDictionary<string, ResourceManager> cachedResourceManagers = new ConcurrentDictionary<string, ResourceManager>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBindingExpression"/> class.
        /// </summary>
        public ResourceBindingExpression()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBindingExpression"/> class.
        /// </summary>
        public ResourceBindingExpression(CompiledBindingExpression expression) : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public object Evaluate(Controls.DotvvmBindableControl control, DotvvmProperty property)
        {
            if (Delegate != null) return Delegate(new object[0], null);

            if (!OriginalString.Contains("."))
            {
                throw new Exception("Invalid resource name! Use Namespace.ResourceType.ResourceKey!");
            }

            // parse expression
            var lastDotPosition = OriginalString.LastIndexOf(".", StringComparison.Ordinal);
            var resourceType = OriginalString.Substring(0, lastDotPosition);
            var resourceKey = OriginalString.Substring(lastDotPosition + 1);

            // find the resource manager
            var resourceManager = cachedResourceManagers.GetOrAdd(resourceType, GetResourceManager);

            // return the value
            return resourceManager.GetString(resourceKey);
        }

        /// <summary>
        /// Gets the resource manager with the specified type name.
        /// </summary>
        private ResourceManager GetResourceManager(string resourceType)
        {
            var typeName = resourceType;
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => new[] {
                    assembly.GetType(typeName),     // the binding can contain full type name
                    assembly.GetType(assembly.GetName().Name + "." + resourceType)      // or the default namespace (which is typically same as assembly name) is omitted
                })
                .FirstOrDefault(t => t != null);

            if (type == null) 
            {
                throw new Exception(string.Format("The resource file '{0}' was not found!", resourceType));
            }
            return (ResourceManager)type.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }
    }
}
