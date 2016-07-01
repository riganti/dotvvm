using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ResourceManagement
{
    public class EmbeddedResourceTranslator
    {
        public static readonly Dictionary<string, string> EmbeddedResourceToAlias;

        static EmbeddedResourceTranslator()
        {
            EmbeddedResourceToAlias = new Dictionary<string, string>
            {
                {"DotVVM.Framework.Resources.Scripts.jquery-2.1.1.min.js", "jquery"},
                {"DotVVM.Framework.Resources.Scripts.knockout-latest.js", "knockout"},
                {"DotVVM.Framework.Resources.Scripts.DotVVM.js", "dotvvm"},
                {"DotVVM.Framework.Resources.Scripts.DotVVM.Debug.js", "dotvvmDebug"},
                {"DotVVM.Framework.Controls.Bootstrap.Resources.dotvvm.Bootstrap.all.js", "bootstrap"},
                {"DotVVM.Framework.Resources.Scripts.DotVVM.FileUpload.css", "fileUpload"},
                {"DotVVM.Framework.Controls.Bootstrap.Resources.Styles.DotVVM.Bootstrap.css", "bootstrapStyles"},
                {"DotVVM.Framework.Resources.Scripts.Globalize.globalize.js", "globalize"}
            };
        }

        public static string TransformUrlToAlias(string url)
        {
            return EmbeddedResourceToAlias.FirstOrDefault(k => k.Key == url).Value;
        }

        public static string TransformAliasToUrl(string alias)
        {
            return EmbeddedResourceToAlias.FirstOrDefault(k => k.Value == alias).Key;
        }

        public static string TransformAssemblyToAlias(string assembly)
        {
            switch (assembly)
            {
                case "DotVVM.Framework":
                    return "dotvvm";
                case "DotVVM.Framework.Controls.Bootstrap":
                    return "bs";
            }

            throw new Exception("Assembly of resource must be only 'DotVVM.Framework' or 'DotVVM.Framework.Controls.Bootstrap'");
        }

        public static string TransformAliasToAssembly(string alias)
        {
            switch (alias)
            {
                case "dotvvm":
                    return "DotVVM.Framework";
                case "bs":
                    return "DotVVM.Framework.Controls.Bootstrap";
            }

            throw new Exception("Alias of resource assembly must be only 'dotvvm' or 'bs'");
        }

        public static bool CheckIfResourceIsInDictionary(string url)
        {
            return EmbeddedResourceToAlias.Any(k => k.Key == url);
        }
    }
}
