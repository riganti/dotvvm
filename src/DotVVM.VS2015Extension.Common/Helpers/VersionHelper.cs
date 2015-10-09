using DotVVM.VS2015Extension.Common.Data;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace DotVVM.VS2015Extension.Common.Helpers
{
    public static class VersionHelper
    {
        private const string RigantiKey = "{823EC690-08F4-4E82-B2A9-01399B8DE0CE}";
        private const string ProductFamilyKey = "{4CBF072E-5204-40A3-A265-DBB5121E0BF5}";
        private const string ProductKey = "{F47175A2-6B14-4B30-8472-38243EB40E79}";

        private static string FullRegistryPath => $"{Registry.CurrentUser.Name}/SOFTWARE/{RigantiKey}/{ProductFamilyKey}/{ProductKey}";

        /// <summary>
        /// Registers installed version of DotVVM Extension, or removes it in case of uninstallation.
        /// </summary>
        /// <param name="visualVersion">version of microsoft visual IDE</param>
        /// <param name="installedVersion">version of newly installed version, or null if uninstalled</param>
        public static void RegisterInstalledVersion(VisualVersionInfo visualVersion, Version installedVersion)
        {
            if (visualVersion == null)
            {
                throw new NullReferenceException($"{nameof(VersionHelper)}.{nameof(RegisterInstalledVersion)} - parameter {nameof(visualVersion)} cannot be null.");
            }
            var extensionKey = GetOrCreateExtensionRegistryKey();
            var binary = SerializationHelper.ObjectToByteArray(installedVersion);
            extensionKey.SetValue(visualVersion.KeyValueName(), binary, RegistryValueKind.Binary);
        }

        /// <summary>
        /// Gets current installed version of DotVVM Extension, if installed.
        /// </summary>
        /// <param name="visualVersion">version of microsoft visual IDE</param>
        /// <returns>extension version, can be null if it's not installed</returns>
        public static Version GetInstalledVersion(VisualVersionInfo visualVersion)
        {
            var extensionKey = GetOrCreateExtensionRegistryKey();
            return SerializationHelper.ByteArrayToObject<Version>(extensionKey.GetValue(visualVersion.KeyValueName(), null) as byte[]);
        }

        public static Version GetVersionFromManifest(string manifestXml)
        {
            var xml = XDocument.Parse(manifestXml);

            // get the default namespace of the document
            var xmlnsNamespace = Regex.Match(manifestXml, "xmlns=\"(?<namespace>[^\"]*)\"");

            // define XML namespace manager and a prefix for the XML namespace used
            var mgr = new XmlNamespaceManager(new NameTable());
            mgr.AddNamespace("xxx", xmlnsNamespace.Groups["namespace"].Success ? xmlnsNamespace.Groups["namespace"].Value : "");

            var xpath = "/xxx:PackageManifest/xxx:Metadata/xxx:Identity/@Version";
            var attribute = ((IEnumerable)xml.XPathEvaluate(xpath, mgr)).Cast<XAttribute>().FirstOrDefault();
            if (attribute == null)
            {
                throw new ArgumentException($"{nameof(VersionHelper)}.{nameof(GetVersionFromManifest)} - invalid xml");
            }
            return new Version(attribute.Value);
        }

        private static RegistryKey GetOrCreateExtensionRegistryKey()
        {
            var pathKeys = FullRegistryPath.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var key = Registry.CurrentUser;
            for (var i = 1; i < pathKeys.Length; i++)
            {
                key = key.CreateSubKey(pathKeys[i]);
            }
            return key;
        }

        private static string KeyValueName(this VisualVersionInfo visualVersion)
        {
            return $"{visualVersion.VisualVersion}_{visualVersion.Version}";
        }
    }
}