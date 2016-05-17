using DotVVM.VS2015Extension.Bases;
using DotVVM.VS2015Extension.Bases.Commands;
using DotVVM.VS2015Extension.Bases.Directives;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification;
using DotVVM.VS2015Extension.DotvvmPageWizard;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Commands.GotoDefinition
{
    internal class DothtmlGotoDefinitionCommandHandler : BaseCommandTarget
    {
        private DothtmlGotoDefinitionHandlerProvider provider;

        public DothtmlGotoDefinitionCommandHandler(IVsTextView textViewAdapter, ITextView textView, DothtmlGotoDefinitionHandlerProvider provider) : base(textViewAdapter, textView, provider)
        {
            this.provider = provider;
        }

        public override Guid CommandGroupId { get; } = typeof(VSConstants.VSStd97CmdID).GUID;
        public override uint[] CommandIds { get; } = { (uint)VSConstants.VSStd97CmdID.GotoDefn };

        protected override bool Execute(uint nCmdId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut, NextIOleCommandTarget nextCommandTarget)
        {
            //check triggered automatic function
            if (VsShellUtilities.IsInAutomationFunction(provider.ServiceProvider))
            {
                var groupId = new Guid(CommandGroupId.ToString());
                return nextCommandTarget.Execute(ref groupId, nCmdId, nCmdexecopt, pvaIn, pvaOut) > 0;
            }
            //get position of cursor
            var position = TextView.Caret.Position.BufferPosition.Position;

            //get root node of syntax tree
            var rootNode = TextView.TextBuffer.GetDothtmlRootNode();

            if (ProcessDirectives(position, rootNode)) return true;

            return false;
        }

        /// <summary>
        /// Checks if directive is selected and returns it.
        /// </summary>
        private static DothtmlDirectiveNode GetCurrentDirective(int position, DothtmlRootNode rootNode)
        {
            return rootNode.Directives.FirstOrDefault(s => s.StartPosition <= position &&
                                                           position <= s.StartPosition + s.Length);
        }

        private bool ProcessDirectives(int position, DothtmlRootNode rootNode)
        {
            var currentDirective = GetCurrentDirective(position, rootNode);
            if (currentDirective == null) return false;

            //check viewModel and typeBased directive and navigate to definition of the viewModel
            if (currentDirective.Name.Equals(ParserConstants.ViewModelDirectiveName, StringComparison.InvariantCultureIgnoreCase) 
                || currentDirective.Name.Equals(ParserConstants.BaseTypeDirective, StringComparison.InvariantCultureIgnoreCase))
            {
                if (NavigateToViewModel(new ViewModelDirectiveValue(currentDirective))) return true;
            }

            //check masterPage directive
            if (currentDirective.Name.Equals(ParserConstants.MasterPageDirective, StringComparison.InvariantCultureIgnoreCase))
            {
                if (NavigateToMasterPage(currentDirective)) return true;
            }
            return false;
        }

        private bool NavigateToMasterPage(DothtmlDirectiveNode masterPageDirective)
        {
            //get full path of item
            var path = DTEHelper.GetCurrentProject().Properties.Item("FullPath").Value;
            var itemFullPath = Path.Combine(path, masterPageDirective.Value);

            //check if item exists
            if (File.Exists(itemFullPath))
            {
                //navigate to item
                var item = DTEHelper.GetProjectItemByFullPath(itemFullPath);
                DTEHelper.ChangeActiveWindowTo(item);
                return true;
            }

            return false;
        }

        private bool NavigateToViewModel(ViewModelDirectiveValue currentDirective)
        {
            //get all declarations of the viewmodel's name
            var declarations = WorkspaceHelper
                .GetSyntaxTreeInfos()
                .SelectMany(
                    s =>
                        s.Tree.GetRoot()
                            .DescendantNodes()
                            .OfType<TypeDeclarationSyntax>()
                            .Select(d => new { DeclarationSyntax = d, Info = s }))
                .Where(s => s.DeclarationSyntax.Identifier.ToString() == currentDirective.TypeName);

            //get exact match
            foreach (var declaration in declarations)
            {
                //declaration.Info.Compilation.GetTypeByMetadataName()

                var semanticModel = declaration.Info.Compilation.GetSemanticModel(declaration.DeclarationSyntax.SyntaxTree);
                var declaredSymbol = semanticModel
                    .GetDeclaredSymbol(declaration.DeclarationSyntax);

                // check assembly name and namespace
                if (declaredSymbol.ContainingAssembly.Identity.Name == currentDirective.AssemblyName
                    && declaredSymbol.ContainingNamespace.ToString() == currentDirective.Namespace)
                {
                    //navigate to definition - open window
                    var item = DTEHelper.GetProjectItemByFullPath(declaration.DeclarationSyntax.Identifier.SyntaxTree.FilePath);
                    DTEHelper.ChangeActiveWindowTo(item);
                    return true;
                }
            }
            return false;
        }
    }
}