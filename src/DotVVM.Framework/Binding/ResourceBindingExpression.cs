using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Parser;
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

        public string FullResourceManagerTypeName { get; set; }
        public string ResourceTypeDirectiveValue { get; set; }
        public string ResourceNamespaceDirectiveValue { get; set; }

        /// <summary>
        /// Evaluates the binding.
        /// </summary>
        public object Evaluate(Controls.DotvvmBindableObject control, DotvvmProperty property)
        {
            if (Delegate != null) return ExecDelegate(control, true);

            // parse expression
            string resourceType = "";

            // check value
            var expressionText = OriginalString?.Trim();
            if (string.IsNullOrWhiteSpace(expressionText))
            {
                throw new Exception("Resource binding contains empty string.");
            }

            GetDirectives(control);

            // check directives combination
            if (!string.IsNullOrWhiteSpace(ResourceTypeDirectiveValue) && !string.IsNullOrWhiteSpace(ResourceNamespaceDirectiveValue))
            { throw new Exception("@resourceType and @resourceNamespace directives cannot be used in the same time!"); }

            // check type directive
            string resourceKey = "";
            if (!string.IsNullOrWhiteSpace(ResourceTypeDirectiveValue))
            {
                if (!ResourceTypeDirectiveValue.Contains(","))
                {
                    throw new Exception($"Assembly of resource type '{expressionText}' was not recognized. Specify make sure the directive value is in format: Assembly, Namespace.ResourceType");
                }
                if (!ResourceTypeDirectiveValue.Contains("."))
                {
                    throw new Exception($"Resource {expressionText} was not found. Specify make sure the directive value is in format: Assembly, Namespace.ResourceType");
                }

                if (!expressionText.Contains("."))
                {
                    resourceType = ResourceTypeDirectiveValue;
                    resourceKey = expressionText;
                }
                else
                {
                    var lastDotPosition = expressionText.LastIndexOf(".", StringComparison.Ordinal);
                    resourceType = expressionText.Substring(0, lastDotPosition);
                    resourceKey = expressionText.Substring(lastDotPosition + 1);
                }
            }
            else if (!string.IsNullOrWhiteSpace(ResourceNamespaceDirectiveValue))
            {
                if (ResourceNamespaceDirectiveValue.Contains(","))
                {
                    throw new Exception($"@resourceNamespace {ResourceNamespaceDirectiveValue} contains unexpected charachter ','");
                }
                if (expressionText.Contains("."))
                {
                    var lastDotPosition = expressionText.LastIndexOf(".", StringComparison.Ordinal);
                    resourceType = expressionText.Substring(0, lastDotPosition);
                    resourceKey = expressionText.Substring(lastDotPosition + 1);
                }
                else
                {
                    throw new Exception($"Resource '{expressionText}' does not specify resource class or resource key. Make sure that format of expression is: OptionalNamespace.ResourceType.ResourceKey");
                }
            }
            else
            {
                if (expressionText.Contains("."))
                {
                    var lastDotPosition = expressionText.LastIndexOf(".", StringComparison.Ordinal);
                    resourceType = expressionText.Substring(0, lastDotPosition);
                    resourceKey = expressionText.Substring(lastDotPosition + 1);
                }
                else
                {
                    throw new Exception($"Resource '{expressionText}' does not specify resource class or resource key. Make sure that format of expression is: Namespace.ResourceType.ResourceKey");
                }
            }
            // find the resource manager
            var resourceManager = cachedResourceManagers.GetOrAdd(resourceType, GetResourceManager);
            if (resourceManager == null)
            {
                resourceManager = cachedResourceManagers.GetOrAdd(ResourceNamespaceDirectiveValue + "." + resourceType, GetResourceManager);
            }
            if (resourceManager == null)
            {
                throw new Exception($"The resource file '{resourceType}' was not found! Make sure that your resource file has Access Modifier setted to Public or Internal.");
            }

            // return the value
            var value = resourceManager.GetString(resourceKey);
            if (value == null)
            {
                throw new Exception($"Resource '{expressionText}' was not found.");
            }
            return value;
        }

        private void GetDirectives(Controls.DotvvmBindableObject control)
        {
            var directives = (Dictionary<string, string>)control.GetValue(DotvvmView.DirectivesProperty);

            // get type directive
            string resourceTypeDirectiveValue;
            directives.TryGetValue(Constants.ResourceTypeDirective, out resourceTypeDirectiveValue);
            ResourceTypeDirectiveValue = (resourceTypeDirectiveValue ?? "").Trim();

            // get namespace directive
            string resourceNamespaceDirectiveValue;
            directives.TryGetValue(Constants.ResourceNamespaceDirective, out resourceNamespaceDirectiveValue);
            ResourceNamespaceDirectiveValue = (resourceNamespaceDirectiveValue ?? "").Trim();
        }

        /// <summary>
        /// Gets the resource manager with the specified type name.
        /// </summary>
        private ResourceManager GetResourceManager(string resourceType)
        {
            Type type;
            List<Type> types;
            if (!string.IsNullOrWhiteSpace(ResourceTypeDirectiveValue) && resourceType == ResourceTypeDirectiveValue)
            {
                type = Type.GetType(ResourceTypeDirectiveValue);
                if (type == null)
                {
                    throw new Exception($"@resourceType '{resourceType}' directive could not be resolved! Make sure that your resource file has Access Modifier setted to Public or Internal.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(resourceType))
                {
                    throw new Exception($"Invalid resource type '{resourceType}'!");
                }
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                types = assemblies.Select(assembly => assembly.GetType(resourceType)).Where(s => s != null).ToList();

                // debug
                if (types.Count > 1)
                {
                    throw new Exception("Ambiguous resource specified in the binding expression. Include the full namespace or use the @resourceType directive to specify correct resource entry.");
                }
                type = types.FirstOrDefault();
            }
            if (type == null) return null;

            var manager = type.GetProperty("ResourceManager") ??
                          type.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static);

            var resourceMananger = (ResourceManager)manager.GetValue(null);
            return resourceMananger;
        }
    }
}