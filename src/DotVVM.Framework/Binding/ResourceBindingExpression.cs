using DotVVM.Framework.Runtime.Compilation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        public object Evaluate(Controls.DotvvmBindableObject control, DotvvmProperty property)
        {
            if (Delegate != null) return ExecDelegate(control, true);

            if (!OriginalString.Contains("."))
            {
                throw new Exception("Invalid resource name! Use Namespace.ResourceType.ResourceKey!");
            }

            // parse expression
            var expressionText = OriginalString.Trim();
            var lastDotPosition = expressionText.LastIndexOf(".", StringComparison.Ordinal);
            var resourceType = expressionText.Substring(0, lastDotPosition);
            var resourceKey = expressionText.Substring(lastDotPosition + 1);

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

            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new Exception("Invalid resource name!");
            }
            var typeName = resourceType;
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => new[] {
                    assembly.GetType(typeName),     // the binding can contain full type name
                    assembly.GetType(assembly.GetName().Name + "." + resourceType)      // or the default namespace (which is typically same as assembly name) is omitted
                }).Where(t => t != null).ToList();

            // debug
            if (types.Count > 1)
            {
                Debug.Indent();
                Debug.Assert(types.Count >1, $"Resource binding contains value ('{OriginalString}') with resource name that is used in more then one assembly.");
                Debug.Unindent();
                Debug.Flush();
            }

            var type = types.FirstOrDefault();

            if (type == null)
            {
                throw new Exception($"The resource file '{resourceType}' was not found! Make sure that your resource file has Access Modifier setted to Public or Internal.");
            }
            var resourceMananger = (ResourceManager)type.GetProperty("ResourceManager").GetValue(null);
            if (resourceMananger == null)
                throw new Exception($"Resource Manager of type {resourceType} was not found.");
            return resourceMananger;
        }


    }
}
