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

        public string AssemblyName { get; set; }
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

                var splittedDirective = ResourceTypeDirectiveValue.Split(',').Select(s => s.Trim()).ToList();
                AssemblyName = splittedDirective[1];
                resourceType = splittedDirective[0];
                resourceKey = expressionText;
            }
            else if (!string.IsNullOrWhiteSpace(ResourceNamespaceDirectiveValue))
            {
                if (!ResourceTypeDirectiveValue.Contains(","))
                {
                    throw new Exception($"@resourceNamespace contains unexpected charachter ','");
                }
                if (expressionText.Contains("."))
                {

                    var lastDotPosition = expressionText.LastIndexOf(".", StringComparison.Ordinal);
                    resourceType = ResourceNamespaceDirectiveValue + "." + expressionText.Substring(0, lastDotPosition);
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

            //get type directive
            string resourceTypeDirectiveValue;
            ((DotvvmView)control.GetRoot()).Directives.TryGetValue(Constants.ResourceTypeDirective,
                 out resourceTypeDirectiveValue);
            ResourceTypeDirectiveValue = (resourceTypeDirectiveValue ?? "").Trim();


            //get namespace directive
            string resourceNamespaceDirectiveValue;
            ((DotvvmView)control.GetRoot()).Directives.TryGetValue(Constants.ResourceNamespaceDirective,
                out resourceNamespaceDirectiveValue);
            ResourceNamespaceDirectiveValue = (resourceNamespaceDirectiveValue ?? "").Trim();
        }


        /// <summary>
        /// Gets the resource manager with the specified type name.
        /// </summary>
        private ResourceManager GetResourceManager(string resourceType)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
            {
                throw new Exception($"Invalid resource type '{resourceType}'!");
            }
            var typeName = resourceType;
            List<Type> types;
            if (string.IsNullOrWhiteSpace(AssemblyName))
            {
                types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => new[] {
                            assembly.GetType(typeName)     // the binding can contain full type name
                        }).Where(t => t != null).ToList();
            }
            else
            {
                var foundAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(assembly => assembly.GetName().Name == AssemblyName);
                if (foundAssembly == null)
                {
                    throw new Exception($"Assembly '{AssemblyName}' specified in @resourceType directive was not found.");
                }
                types = new[]
                {
                    foundAssembly.GetType(typeName), // the binding can contain full type name
                    foundAssembly.GetType(AssemblyName + "." + resourceType)
                    // or the default namespace (which is typically same as assembly name) is omitted
                }.Where(t => t != null).ToList();
            }

            // debug
            if (types.Count > 1)
            {
                throw new Exception("Ambiguous resource specified in the binding expression. Include the full namespace or use the @resourceType directive to specify correct resource entry.");
            }

            var type = types.FirstOrDefault();

            if (type == null)
            {
                throw new Exception($"The resource file '{resourceType}' was not found! Make sure that your resource file has Access Modifier setted to Public or Internal.");
            }

            var manager = type.GetProperty("ResourceManager") ??
                          type.GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Static);

            var resourceMananger = (ResourceManager)manager.GetValue(null);
            if (resourceMananger == null)
                throw new Exception($"Resource Manager of type {resourceType} was not found.");
            return resourceMananger;
        }
    }
}