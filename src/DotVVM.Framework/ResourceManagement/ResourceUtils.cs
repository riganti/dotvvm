using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
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
    }
}
