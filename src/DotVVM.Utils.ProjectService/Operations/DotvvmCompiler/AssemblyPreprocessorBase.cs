using System.Xml;
using System.Xml.Linq;

namespace DotVVM.Utils.ProjectService.Operations.DotvvmCompiler
{
    public abstract class AssemblyPreprocessorBase : IAssemblyPreprocessor
    {
        protected const string AssemblyIdentityNode = "assemblyIdentity";
        protected const string DependentAssemblyNode = "dependentAssembly";
        protected const string BindingRedirectNode = "bindingRedirect";
        protected const string AssemblyBindingNode = "assemblyBinding";
        protected const string NewVersionAttribute = "newVersion";
        protected const string OldVersionAttribute = "oldVersion";
        protected const string NameAttribute = "name";
        protected IResolvedProjectMetadata Metadata { get; }
        protected string CompilerPath { get; }
        protected XNamespace Ns { get; } = "urn:schemas-microsoft-com:asm.v1";
        protected XmlNamespaceManager NsManager { get; }
        protected XDocument CompilerAppConfig { get; }
        protected string CompilerAppConfigPath { get; }

        protected AssemblyPreprocessorBase(IResolvedProjectMetadata metadata, string compilerPath)
        {
            Metadata = metadata;
            CompilerPath = compilerPath;

            NsManager = new XmlNamespaceManager(new NameTable());
            NsManager.AddNamespace("ns", Ns.NamespaceName);

            CompilerAppConfigPath = compilerPath + ".config";
            CompilerAppConfig = XDocument.Load(CompilerAppConfigPath);
        }
        public abstract void CreateBindings();

        protected void SaveCompilerConfig()
        {
            CompilerAppConfig.Save(CompilerAppConfigPath);
        }

        protected void ProcessDefaultCompilerConfig()
        {
            var compilerBindings = CompilerAppConfig.Descendants(Ns + DependentAssemblyNode);
            foreach (var dependentAssembly in compilerBindings)
            {
                var assemblyIdentity = dependentAssembly.Element(Ns + AssemblyIdentityNode);
                var bindingRedirect = dependentAssembly.Element(Ns + BindingRedirectNode);
                ReplaceBindingOldVersion(bindingRedirect);
            }
        }
        protected void ReplaceBindingOldVersion(XElement bindingRedirectNode)
        {
            bindingRedirectNode.Attribute(OldVersionAttribute).Value = Constants.MaxAssemblyVersionRange;
        }
        protected void ReplaceBindingNewVersion(XElement dependentAssemblyNodeToReplace, XElement bindingRedirectNodeReplaceWith)
        {
            dependentAssemblyNodeToReplace.Element(Ns + BindingRedirectNode).Attribute(NewVersionAttribute).Value =
                bindingRedirectNodeReplaceWith.Attribute(NewVersionAttribute).Value;
        }
    }
}