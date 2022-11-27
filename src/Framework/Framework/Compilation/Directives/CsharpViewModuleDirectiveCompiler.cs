using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ViewCompiler;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Framework.Compilation.Directives;

public class CsharpViewModuleDirectiveCompiler : DirectiveCompiler<IAbstractCsharpViewModuleDirective, CSharpViewModuleCompilationResult?>
{
    private readonly IAbstractControlBuilderDescriptor? masterPage;
    private readonly bool isMarkupControl;
    private readonly ImmutableList<NamespaceImport> imports;

    public CsharpViewModuleDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, IAbstractControlBuilderDescriptor? masterPage, bool isMarkupControl, ImmutableList<NamespaceImport> imports)
        : base(directiveNodesByName, treeBuilder)
    {
        this.masterPage = masterPage;
        this.isMarkupControl = isMarkupControl;
        this.imports = imports;
    }

    public override string DirectiveName => ParserConstants.CsharpViewModuleDirective;

    protected override CSharpViewModuleCompilationResult? CreateArtefact(IReadOnlyList<IAbstractCsharpViewModuleDirective> resolvedDirectives)
    {
        var id = AssignViewModuleId(masterPage);
        return ResolveImportedViewModules(resolvedDirectives, id);
    }

    private CSharpViewModuleCompilationResult? ResolveImportedViewModules(IReadOnlyList<IAbstractCsharpViewModuleDirective> moduleDirectives, string id)
    {
        if (moduleDirectives.Count == 0)
        {
            return null;
        }

        if (moduleDirectives.Count > 1)
        {
            moduleDirectives[1].DothtmlNode!.AddError("There can be only one @csharp directive in the page!");
            return null;
        }

        var x = moduleDirectives[0];
        if (x.ModuleType == null)
        {
            moduleDirectives[0].DothtmlNode!.AddError($"The type {moduleDirectives[0].Value} was not found. Make sure the namespace and assembly name is correct.");
            return null;
        }

        var info = new ViewModuleReferenceInfo(
            id,
            new[] { new ViewModuleReferencedModule(ResourceConstants.DotvvmDotnetWasmInteropResourceName, new[] { x.ModuleType.FullName + ", "+ x.ModuleType.Assembly }) },
            isMarkupControl);

        return new CSharpViewModuleCompilationResult(new CSharpExtensionParameter(id, isMarkupControl, moduleDirectives[0].ModuleType), info);
    }

    protected virtual string AssignViewModuleId(IAbstractControlBuilderDescriptor? masterPage)
    {
        var numberOfMasterPages = 0;
        while (masterPage != null)
        {
            masterPage = masterPage.MasterPage;
            numberOfMasterPages += 1;
        }
        return "p" + numberOfMasterPages;
    }

    protected override IAbstractCsharpViewModuleDirective Resolve(DothtmlDirectiveNode directiveNode) =>
        TreeBuilder.BuildCsharpViewModuleDirective(directiveNode, ParseDirective(directiveNode, p => p.ReadDirectiveTypeName()), imports);
}
