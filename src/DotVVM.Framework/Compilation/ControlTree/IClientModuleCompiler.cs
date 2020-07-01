using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public interface IClientModuleCompiler
    {
        ClientModuleExtensionParameter GetClientModuleExtensionParameter(MarkupFile fileName, IControlResolverMetadata viewMetadata, ITypeDescriptor viewModelType, ImmutableList<NamespaceImport> namespaceImports, ImmutableList<InjectedServiceExtensionParameter> injectedServices, DothtmlElementNode clientModuleNode);

        void PrepareClientModuleResource(IControlResolverMetadata viewMetadata, IDataContextStack dataContextTypeStack);

        string GetClientModuleResourceScript(string clientModuleName);

        string TryGetClientModuleResourceName(ControlResolverMetadata viewMetadata);
    }
}
