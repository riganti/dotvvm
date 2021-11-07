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
                var currentResourceName = stack.Peek();

                var currentResource = currentResourceName == name
                    ? resource
                    : findResource(currentResourceName);

                var isCurrentResourceProcessingFinished =
                    currentResource == null ||
                    ProcessDependencies(name, stack, visited, currentResource);

                if (isCurrentResourceProcessingFinished)
                {
                    visited.Add(currentResourceName);
                    stack.Pop();
                }
            }
        }

        private static bool ProcessDependencies(string name, Stack<string> stack, HashSet<string> visited, IResource currentResource)
        {
            var nextDependencyIdentifier = currentResource.Dependencies.FirstOrDefault(d => !visited.Contains(d));
            if (nextDependencyIdentifier != null)
            {
                if (stack.Contains(nextDependencyIdentifier))
                {
                    var dependencyChain = stack
                        .Reverse()
                        .Concat(new[] { nextDependencyIdentifier })
                        .ToArray();

                    throw new DotvvmCyclicResourceDependencyException(name, currentResource, dependencyChain);
                }

                stack.Push(nextDependencyIdentifier);
                return false;
            }

            return true;
        }
    }
}
