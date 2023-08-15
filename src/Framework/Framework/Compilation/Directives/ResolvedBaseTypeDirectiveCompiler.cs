using System;
using System.Collections.Generic;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Immutable;

namespace DotVVM.Framework.Compilation.Directives
{
    public class ResolvedBaseTypeDirectiveCompiler : BaseTypeDirectiveCompiler
    {
        private static readonly Lazy<ModuleBuilder> DynamicMarkupControlAssembly = new(CreateDynamicMarkupControlAssembly);

        public ResolvedBaseTypeDirectiveCompiler(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName, IAbstractTreeBuilder treeBuilder, string fileName, ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder, fileName, imports)
        {
        }

        protected override ITypeDescriptor? GetOrCreateDynamicType(ITypeDescriptor baseType, string typeName)
        {
            if (DynamicMarkupControlAssembly.Value.GetType(typeName) is { } type)
            {
                return new ResolvedTypeDescriptor(type);
            }

            var declaringTypeBuilder =
                DynamicMarkupControlAssembly.Value.DefineType(typeName, TypeAttributes.Public, ResolvedTypeDescriptor.ToSystemType(baseType));
            var createdTypeInfo = declaringTypeBuilder.CreateTypeInfo()?.AsType();

            return createdTypeInfo is not null
                ? new ResolvedTypeDescriptor(createdTypeInfo)
                : null;
        }

        private static ModuleBuilder CreateDynamicMarkupControlAssembly()
        {
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
            return mb;
        }
    }

}
