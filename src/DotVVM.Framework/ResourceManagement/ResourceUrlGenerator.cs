using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement.ClientGlobalize;

namespace DotVVM.Framework.ResourceManagement
{
    internal static class ResourceUrlGenerator
    {
        private static readonly ConcurrentDictionary<string, string> ResourceHashDictionary = new ConcurrentDictionary<string, string>(); 

        public static string GetEmbeddedResourceUrl(string resourceName, string assemblyName)
        {
            string key = resourceName;
            if (ResourceHashDictionary.ContainsKey(key))
                return ResourceHashDictionary[key];

            string resourceUrl = String.Format(HostingConstants.ResourceHandlerUrl, WebUtility.UrlEncode(resourceName), WebUtility.UrlEncode(assemblyName));
            ResourceHashDictionary[key] = resourceUrl;
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == assemblyName);
            if (assembly != null)
            {
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        string hash = GetHashString(stream);
                        ResourceHashDictionary[key] = $"{resourceUrl}&hash={hash}";
                    }
                }
            }
            return ResourceHashDictionary[key];
        }

        public static string GetResourceUrl(IDotvvmRequestContext context, string resourceUrl)
        {
            string key = resourceUrl;
            if (ResourceHashDictionary.ContainsKey(key))
                return ResourceHashDictionary[key];

            ResourceHashDictionary[key] = resourceUrl;

            string applicationPhysicalPath = Path.GetFullPath(context.Configuration.ApplicationPhysicalPath);
            string resourcePath = context.TranslateVirtualPath(resourceUrl).Replace("/", @"\");
            if (Path.IsPathRooted(resourcePath))
            {
                resourcePath = resourcePath.TrimStart(Path.DirectorySeparatorChar);
                resourcePath = resourcePath.TrimStart(Path.AltDirectorySeparatorChar);
            }

            resourcePath = System.IO.Path.Combine(applicationPhysicalPath, resourcePath);
            if (File.Exists(resourcePath))
            {
                using (FileStream fs = new FileStream(resourcePath, FileMode.Open))
                {
                    string hash = GetHashString(fs);
                    ResourceHashDictionary[key] = $"{resourceUrl}?hash={hash}";
                }
            }
            return ResourceHashDictionary[key];
        }

        public static string GetGlobalizeCultureResourceUrl(string cultureName)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
            string url = $"~/{HostingConstants.GlobalizeCultureUrlPath}?{HostingConstants.GlobalizeCultureUrlIdParameter}={cultureName}";
            string key = url;
            if (ResourceHashDictionary.ContainsKey(key))
                return ResourceHashDictionary[key];

            var js = JQueryGlobalizeScriptCreator.BuildCultureInfoScript(culture);
            string hash = GetHashString(js);
            ResourceHashDictionary[key] = $"{url}&hash={hash}";

            return ResourceHashDictionary[key];
        }

        private static string GetHashString(Stream stream)
        {
            using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
            {
                var hash = provider.ComputeHash(stream);
                return ((uint)Convert.ToBase64String(hash).GetHashCode()).ToString();
            }
        }

        private static string GetHashString(string str)
        {
            return ((uint)str.GetHashCode()).ToString();
        }
    }
}