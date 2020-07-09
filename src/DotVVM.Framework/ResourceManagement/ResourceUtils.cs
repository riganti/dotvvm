#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotVVM.Framework.ResourceManagement
{
    public static class ResourceUtils
    {
        public static string AddTemplateResource(this ResourceManager manager, IDotvvmRequestContext context, DotvvmControl control)
        {
            using (var text = new StringWriter())
            {
                control.Render(new HtmlWriter(text, context), context);
                return manager.AddTemplateResource(text.ToString());
            }
        }

        public static string ReadToString(this ILocalResourceLocation location, IDotvvmRequestContext context)
        {
            using (var resourceStream = location.LoadResource(context))
            {
                using (var resourceStreamReader = new StreamReader(resourceStream))
                {
                    return resourceStreamReader.ReadToEnd();
		}
            }
	}

        public static void AssertAcyclicDependencies(IResource resource,
            string name,
            Func<string, IResource?> findResource)
        {
            if (resource.Dependencies.Length == 0)
                return;

            var firstDependency = resource.Dependencies.First();
            var stack = new Stack<string>();
            var visited = new HashSet<string>() { firstDependency };
            stack.Push(firstDependency);

            while (stack.Count > 0)
            {
                var currentResource = findResource(stack.Pop());
                if (currentResource == null)
                    continue;

                foreach (var dependencyIdentifier in currentResource.Dependencies)
                {
                    var currentDependency = findResource(dependencyIdentifier);
                    if (currentDependency == null || currentDependency.Dependencies.Length == 0)
                        continue;

                    if (visited.Contains(dependencyIdentifier))
                    {
                        // dependency cycle detected
                        throw new DotvvmResourceException($"Resource \"{name}\" has a cyclic " +
                            $"dependency.");
                    }

                    visited.Add(dependencyIdentifier);
                    stack.Push(dependencyIdentifier);
                }
            }
        }
    }
}
