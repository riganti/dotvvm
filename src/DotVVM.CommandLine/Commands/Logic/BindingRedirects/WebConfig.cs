using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DotVVM.Utils.ProjectService;
using DotVVM.Utils.ProjectService.Lookup;

namespace DotVVM.CommandLine.Commands.Logic.BindingRedirects
{
    [XmlRoot(ElementName = "configuration")]
    public class WebConfig
    {
        [XmlElement(ElementName = "appSettings")]
        public AppSettingsSection AppSettings { get; set; }
        [XmlElement(ElementName = "system.web")]
        public SystemWebSection SystemWeb { get; set; }
        [XmlElement(ElementName = "system.webServer")]
        public SystemWebServerSection SystemWebServer { get; set; }
        [XmlElement(ElementName = "runtime")]
        public RuntimeSection Runtime { get; set; }

        [System.Xml.Serialization.XmlRoot(ElementName = "add")]
        public class Add
        {
            [XmlAttribute(AttributeName = "key")]
            public string Key { get; set; }
            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }
        }

        [XmlRoot(ElementName = "appSettings")]
        public class AppSettingsSection
        {
            [XmlElement(ElementName = "add")]
            public Add Add { get; set; }
        }

        [XmlRoot(ElementName = "compilation")]
        public class Compilation
        {
            [XmlAttribute(AttributeName = "debug")]
            public string Debug { get; set; }
            [XmlAttribute(AttributeName = "targetFramework")]
            public string TargetFramework { get; set; }
        }

        [XmlRoot(ElementName = "httpRuntime")]
        public class HttpRuntime
        {
            [XmlAttribute(AttributeName = "targetFramework")]
            public string TargetFramework { get; set; }
        }

        [XmlRoot(ElementName = "system.web")]
        public class SystemWebSection
        {
            [XmlElement(ElementName = "compilation")]
            public Compilation Compilation { get; set; }
            [XmlElement(ElementName = "httpRuntime")]
            public HttpRuntime HttpRuntime { get; set; }
        }

        [XmlRoot(ElementName = "modules")]
        public class Modules
        {
            [XmlAttribute(AttributeName = "runAllManagedModulesForAllRequests")]
            public string RunAllManagedModulesForAllRequests { get; set; }
        }

        [XmlRoot(ElementName = "validation")]
        public class Validation
        {
            [XmlAttribute(AttributeName = "validateIntegratedModeConfiguration")]
            public string ValidateIntegratedModeConfiguration { get; set; }
        }

        [XmlRoot(ElementName = "system.webServer")]
        public class SystemWebServerSection
        {
            [XmlElement(ElementName = "modules")]
            public Modules Modules { get; set; }
            [XmlElement(ElementName = "validation")]
            public Validation Validation { get; set; }
        }

        [XmlRoot(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v1")]
        public class AssemblyIdentity
        {
            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }
            [XmlAttribute(AttributeName = "publicKeyToken")]
            public string PublicKeyToken { get; set; }
            [XmlAttribute(AttributeName = "culture")]
            public string Culture { get; set; }
        }

        [XmlRoot(ElementName = "bindingRedirect", Namespace = "urn:schemas-microsoft-com:asm.v1")]
        public class BindingRedirect
        {
            [XmlAttribute(AttributeName = "oldVersion")]
            public string OldVersion { get; set; }
            [XmlAttribute(AttributeName = "newVersion")]
            public string NewVersion { get; set; }
        }

        [XmlRoot(ElementName = "dependentAssembly", Namespace = "urn:schemas-microsoft-com:asm.v1")]
        public class DependentAssembly
        {
            [XmlElement(ElementName = "assemblyIdentity", Namespace = "urn:schemas-microsoft-com:asm.v1")]
            public AssemblyIdentity AssemblyIdentity { get; set; }
            [XmlElement(ElementName = "bindingRedirect", Namespace = "urn:schemas-microsoft-com:asm.v1")]
            public BindingRedirect BindingRedirect { get; set; }
        }

        [XmlRoot(ElementName = "assemblyBinding", Namespace = "urn:schemas-microsoft-com:asm.v1")]
        public class AssemblyBinding
        {
            [XmlElement(ElementName = "dependentAssembly", Namespace = "urn:schemas-microsoft-com:asm.v1")]
            public List<DependentAssembly> DependentAssemblies { get; set; }
            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }
        }

        [XmlRoot(ElementName = "runtime")]
        public class RuntimeSection
        {
            [XmlElement(ElementName = "assemblyBinding", Namespace = "urn:schemas-microsoft-com:asm.v1")]
            public AssemblyBinding AssemblyBinding { get; set; }
        }

        public static string Locate(string filePath)
        {
            var csproj = new FileInfo(filePath);
            var webConfig = csproj.Directory.GetFiles("Web.Config").SingleOrDefault();
            return webConfig?.FullName;
        }

        public static WebConfig Load(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(WebConfig));
            using (var reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
            {
                return (WebConfig)serializer.Deserialize(reader);
            }
        }

        public static void Save(WebConfig config, string path)
        {
            var xmlSerializer = new XmlSerializer(typeof(WebConfig));
            using (var streamWriter = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
            {
                xmlSerializer.Serialize(streamWriter, config);
            }
        }

        public static void UpdateRedirects(WebConfig config, IResolvedProjectMetadata project)
        {
            foreach (var dependency in project.DotvvmProjectDependencies)
            {
                var redirect = config.Runtime.AssemblyBinding.DependentAssemblies
                    .SingleOrDefault(a => a.AssemblyIdentity.Name == dependency.Name);
                if (redirect.BindingRedirect.NewVersion.CompareTo(dependency.Name) < 0)
                {
                    redirect.BindingRedirect.NewVersion = dependency.Version;
                    redirect.BindingRedirect.OldVersion = $"0.0.0-{dependency.Version}";
                }
            }
        }

        private DependentAssembly CreateDependentAssembly(ProjectDependency dependency)
        {
            var assembly = new DependentAssembly();
            var identity = new AssemblyIdentity { Name = dependency.Name, Culture = "neutral" };
            var redirect = new BindingRedirect { NewVersion = dependency.Version, OldVersion = $"0.0.0-{dependency.Version}" };
            assembly.AssemblyIdentity = identity;
            assembly.BindingRedirect = redirect;
            return assembly;
        }
    }
}
