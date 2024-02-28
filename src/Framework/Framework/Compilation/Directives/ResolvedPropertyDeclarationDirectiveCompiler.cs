using System.Collections.Immutable;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using System;
using System.Reflection.Emit;
using System.Reflection;

namespace DotVVM.Framework.Compilation.Directives
{
    public class ResolvedPropertyDeclarationDirectiveCompiler : PropertyDeclarationDirectiveCompiler
    {
        private static readonly Lazy<ModuleBuilder> DynamicMarkupControlAssembly = new(CreateDynamicMarkupControlAssembly);

        public ResolvedPropertyDeclarationDirectiveCompiler(
            ImmutableDictionary<string, ImmutableList<DothtmlDirectiveNode>> directiveNodesByName,
            IAbstractTreeBuilder treeBuilder, ITypeDescriptor controlWrapperType,
            ImmutableList<NamespaceImport> imports)
            : base(directiveNodesByName, treeBuilder, controlWrapperType, imports)
        {
        }

        protected override bool HasPropertyType(IAbstractPropertyDeclarationDirective directive)
           => directive.PropertyType is ResolvedTypeDescriptor { Type: not null };

        protected override IPropertyDescriptor TryCreateDotvvmPropertyFromDirective(IAbstractPropertyDeclarationDirective propertyDeclarationDirective)
        {
            if (propertyDeclarationDirective.PropertyType is not ResolvedTypeDescriptor { Type: not null } propertyType) { throw new ArgumentException("propertyDeclarationDirective.PropertyType must be of type ResolvedTypeDescriptor and have non null type."); }
            if (propertyDeclarationDirective.DeclaringType is not ResolvedTypeDescriptor { Type: not null } declaringType) { throw new ArgumentException("propertyDeclarationDirective.DeclaringType must be of type ResolvedTypeDescriptor and have non null type."); }

            return DotvvmProperty.Register(
                propertyDeclarationDirective.NameSyntax.Name,
                propertyType.Type,
                declaringType.Type,
                propertyDeclarationDirective.InitialValue,
                false,
                null,
                propertyDeclarationDirective,
                false);
        }

        protected override ITypeDescriptor? GetOrCreateDynamicType(
            ITypeDescriptor baseType,
            string typeName,
            ImmutableList<IAbstractPropertyDeclarationDirective> propertyDirectives)
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
