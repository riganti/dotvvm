#nullable enable
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;

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
            var queue = new Queue<string>();
            foreach (var dependency in resource.Dependencies)
            {
                queue.Enqueue(dependency);
            }
            while (queue.Count > 0)
            {
                var currentName = queue.Dequeue();
                var current = findResource(currentName);

                if (current is null)
                    continue;

                if (resource == current)
                {
                    // dependency cycle detected
                    throw new DotvvmResourceException($"Resource \"{name}\" has a cyclic " +
                        $"dependency.");
                }
                foreach (var dependency in current.Dependencies)
                {
                    queue.Enqueue(dependency);
                }
            }
        }
    }
}
