using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Controls;
using System.Reflection;
using System.Reflection.Emit;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Utils;
using System.Linq;
using DotVVM.Framework.Compilation.ViewCompiler;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Directives
{
    public class BaseTypeDirectiveCompiler : DirectiveCompiler<IAbstractBaseTypeDirective, ITypeDescriptor>
    {
        private readonly string fileName;
        private readonly ImmutableList<NamespaceImport> imports;

        public override string DirectiveName => ParserConstants.BaseTypeDirective;

        public BaseTypeDirectiveCompiler(
            IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, string fileName, ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder)
        {
            this.fileName = fileName;
            this.imports = imports;
        }

        protected override IAbstractBaseTypeDirective Resolve(DothtmlDirectiveNode directiveNode)
            => TreeBuilder.BuildBaseTypeDirective(directiveNode, ParseDirective(directiveNode, p => p.ReadDirectiveTypeName()), imports);

        protected override ITypeDescriptor CreateArtefact(IReadOnlyList<IAbstractBaseTypeDirective> resolvedDirectives) {
            var wrapperType = GetDefaultWrapperType();

            var baseControlDirective = resolvedDirectives.SingleOrDefault();

            if (baseControlDirective != null)
            {
                var baseType = baseControlDirective.ResolvedType;
                if (baseType == null)
                {
                    baseControlDirective.DothtmlNode!.AddError($"The type '{baseControlDirective.Value}' specified in baseType directive was not found!");
                }
                else if (!baseType.IsAssignableTo(new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl))))
                {
                    baseControlDirective.DothtmlNode!.AddError("Markup controls must derive from DotvvmMarkupControl class!");
                    wrapperType = baseType;
                }
                else
                {
                    wrapperType = baseType;
                }
            }

            if (DirectiveNodesByName.TryGetValue(ParserConstants.PropertyDeclarationDirective, out var abstractDirectives) && abstractDirectives.Any())
            {
                wrapperType = CreateDymanicDeclaringType(wrapperType) ?? wrapperType;
            }

            return wrapperType;
        }

        protected virtual ITypeDescriptor? CreateDymanicDeclaringType(ITypeDescriptor? originalWrapperType)
        {
            var baseType = originalWrapperType?.CastTo<ResolvedTypeDescriptor>().Type ?? typeof(DotvvmMarkupControl);
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);

            var newDynamicAssemblyName = $"DotvvmMarkupControlDynamicAssembly-{Guid.NewGuid()}";
            var assemblyName = new AssemblyName(newDynamicAssemblyName);
            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);

            // For a single-module assembly, the module name is usually
            // the assembly name plus an extension.
            var mb =
                assemblyBuilder.DefineDynamicModule(newDynamicAssemblyName);

            var declaringTypeBuilder = mb.DefineType(
                $"DotvvmMarkupControl-{Guid.NewGuid()}",
                 TypeAttributes.Public, baseType);

            var createdTypeInfo = declaringTypeBuilder.CreateTypeInfo()?.AsType(); 

            return createdTypeInfo is not null
                ? new ResolvedTypeDescriptor(createdTypeInfo)
                : null;
        }

        /// <summary>
        /// Gets the default type of the wrapper for the view.
        /// </summary>
        private ITypeDescriptor GetDefaultWrapperType()
        {
            ITypeDescriptor wrapperType;
            if (fileName.EndsWith(".dotcontrol", StringComparison.Ordinal))
            {
                wrapperType = new ResolvedTypeDescriptor(typeof(DotvvmMarkupControl));
            }
            else
            {
                wrapperType = new ResolvedTypeDescriptor(typeof(DotvvmView));
            }
            return wrapperType;
        }
    }

}
