#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Utils;
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

            var stack = new Stack<string>();
            var visited = new HashSet<string>();
            stack.Push(name);

            while (stack.Count > 0)
            {
                var currentResourceIdentifier = stack.Peek();
                var currentResource = findResource(currentResourceIdentifier);

                var isProcessed = true;
                if (currentResource != null)
                {
                    var nextDependencyIdentifier = currentResource.Dependencies.FirstOrDefault(d => !visited.Contains(d));
                    if (nextDependencyIdentifier != null)
                    {
                        if (stack.Contains(nextDependencyIdentifier))
                        {
                            // dependency cycle detected
                            throw new DotvvmResourceException($"Resource \"{name}\" has a cyclic " +
                                $"dependency: {stack.Reverse().StringJoin(" --> ")} --> {nextDependencyIdentifier}");
                        }

                        stack.Push(nextDependencyIdentifier);
                        isProcessed = false;
                    }
                }

                if (isProcessed)
                {
                    visited.Add(currentResourceIdentifier);
                    stack.Pop();
                }
            }
        }
    }
}
