using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Redwood.Framework.Binding
{
    public class ResourceBindingExpression : BindingExpression
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
        public ResourceBindingExpression(string expression) : base(expression)
        {
        }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public override object Evaluate(Controls.RedwoodBindableControl control, RedwoodProperty property)
        {
            if (!Expression.Contains("."))
            {
                throw new Exception("Invalid resource name! Use Namespace.ResourceType.ResourceKey!");
            }

            // parse expression
            var lastDotPosition = Expression.LastIndexOf(".");
            var resourceType = Expression.Substring(0, lastDotPosition);
            var resourceKey = Expression.Substring(lastDotPosition + 1);

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


        public override string TranslateToClientScript(Controls.RedwoodBindableControl control, RedwoodProperty property)
        {
            throw new NotSupportedException();
        }
    }
}
