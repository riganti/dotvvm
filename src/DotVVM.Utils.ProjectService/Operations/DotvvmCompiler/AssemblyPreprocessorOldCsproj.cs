using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using DotVVM.Utils.ProjectService.Extensions;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public class AssemblyPreprocessorOldCsproj : AssemblyPreprocessorBase //name?
    {
        private const string PublicKeyTokenAttribute = "publicKeyToken";
        private const string CultureAttribute = "culture";
        private XDocument WebConfig { get; }

        public AssemblyPreprocessorOldCsproj(IResolvedProjectMetadata metadata, string compilerPath) : base(metadata, compilerPath)
        {
            var webConfigPath = Path.Combine(Path.GetDirectoryName(metadata.CsprojFullName), "Web.config");
            if (!File.Exists(webConfigPath)) return;
            WebConfig = XDocument.Load(webConfigPath);
        }

        public override void CreateBindings()
        {
            ProcessDefaultCompilerConfig();
            ProcessWebAssemblyReferences();
            ProcessWebConfig();
            SaveCompilerConfig();
        }

        private void ProcessWebAssemblyReferences()
        {
            var compilerAssemblyBindingNode = GetCompilerAssemblyBindingNode();
            foreach (var assembly in Assembly.ReflectionOnlyLoadFrom(Metadata.GetWebsiteAssemblyPath()).GetReferencedAssemblies())
            {
                compilerAssemblyBindingNode
                    .Add(new XElement(Ns + DependentAssemblyNode,
                            GetAssemblyIdentityElement(assembly),
                            GetBindingRedirectElement(assembly)));
            }
        }

        private XElement GetBindingRedirectElement(AssemblyName assembly)
        {
            return new XElement(Ns + BindingRedirectNode,
                new XAttribute(OldVersionAttribute, Constants.MaxAssemblyVersionRange),
                new XAttribute(NewVersionAttribute, assembly.Version));
        }

        private XElement GetAssemblyIdentityElement(AssemblyName assembly)
        {
            var splitFullName = assembly.FullName.Split(',');
            var publicKeyToken = splitFullName.FirstOrDefault(s => s.Contains("PublicKeyToken"))
                ?.Trim().Replace("PublicKeyToken=", "");
            var culture = splitFullName.FirstOrDefault(s => s.Contains("Culture"))
                ?.Trim().Replace("Culture=", "");
            return new XElement(Ns + AssemblyIdentityNode,
                new XAttribute(NameAttribute, assembly.Name),
                new XAttribute(PublicKeyTokenAttribute, publicKeyToken ?? ""),
                new XAttribute(CultureAttribute, culture ?? ""));
        }

        private void ProcessWebConfig()
        {
            if (WebConfig == null) return;
            var webBindings = WebConfig.Descendants(Ns + DependentAssemblyNode).ToList();
            if (!webBindings.Any()) return;

            var compilerAssemblyBindingNode = GetCompilerAssemblyBindingNode();

            foreach (var dependentAssembly in webBindings)
            {
                var assemblyIdentity = dependentAssembly.Element(Ns + AssemblyIdentityNode);
                var bindingRedirect = dependentAssembly.Element(Ns + BindingRedirectNode);
                var assemblyName = assemblyIdentity.Attribute(NameAttribute).Value;
                var existingNode = CompilerAppConfig
                    .XPathSelectElement($"//ns:{AssemblyIdentityNode}[@{NameAttribute}='{assemblyName}']", NsManager);
                if (existingNode != null)
                {
                    ReplaceBindingNewVersion(existingNode.Parent, bindingRedirect);
                }
                else
                {
                    ReplaceBindingOldVersion(bindingRedirect);

                    compilerAssemblyBindingNode.Add(dependentAssembly);
                }
            }
        }

        private XElement GetCompilerAssemblyBindingNode()
        {
            return CompilerAppConfig.Descendant(Ns + AssemblyBindingNode);
        }
    }
}
