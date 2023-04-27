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
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;

namespace DotVVM.Framework.Compilation.Directives
{
    public class BaseTypeDirectiveCompiler : DirectiveCompiler<IAbstractBaseTypeDirective, ITypeDescriptor>
    {
        private static readonly Lazy<ModuleBuilder> DynamicMarkupControlAssembly = new (CreateDynamicMarkupControlAssembly);

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
            var isMarkupControl = IsMarkupControl();
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
                else if (baseType.GetControlMarkupOptionsAttribute() is { } attribute
                         && (!string.IsNullOrEmpty(attribute.PrimaryName) || attribute.AlternativeNames?.Any() == true))
                {
                    baseControlDirective.DothtmlNode!.AddError("Markup controls cannot use the PrimaryName or AlternativeNames properties in the ControlMarkupOptions attribute!");
                    wrapperType = baseType;
                }
                else
                {
                    wrapperType = baseType;
                }
            }

            if (isMarkupControl && DirectiveNodesByName.TryGetValue(ParserConstants.PropertyDeclarationDirective, out var propertyDirectives) && propertyDirectives.Any())
            {
                wrapperType = CreateDynamicDeclaringType(wrapperType, propertyDirectives) ?? wrapperType;
            }

            return wrapperType;
        }

        /// <summary> Gets or creates dynamic declaring type, and registers on it the properties declared using `@property` directives </summary>
        protected virtual ITypeDescriptor? CreateDynamicDeclaringType(
            ITypeDescriptor? originalWrapperType,
            IEnumerable<DothtmlDirectiveNode> propertyDirectives
        )
        {
            var imports = DirectiveNodesByName.GetValueOrDefault(ParserConstants.ImportNamespaceDirective, Array.Empty<DothtmlDirectiveNode>())
                .Select(d => d.Value.Trim()).OrderBy(s => s).ToImmutableArray();
            var properties = propertyDirectives
                .Select(p => p.Value.Trim()).OrderBy(s => s).ToImmutableArray();
            var baseType = originalWrapperType?.CastTo<ResolvedTypeDescriptor>().Type ?? typeof(DotvvmMarkupControl);

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(
                new UTF8Encoding(false).GetBytes(
                    baseType.FullName + "||" + string.Join("|", imports) + "||" + string.Join("|", properties)
                )
            );
            var hash = Convert.ToBase64String(hashBytes, 0, 16);

            var typeName = "DotvvmMarkupControl-" + hash;
            if (DynamicMarkupControlAssembly.Value.GetType(typeName) is { } type)
            {
                return new ResolvedTypeDescriptor(type);
            }

            var declaringTypeBuilder =
                DynamicMarkupControlAssembly.Value.DefineType(typeName, TypeAttributes.Public, baseType);
            var createdTypeInfo = declaringTypeBuilder.CreateTypeInfo()?.AsType();

            return createdTypeInfo is not null
                ? new ResolvedTypeDescriptor(createdTypeInfo)
                : null;
        }

        /// <summary>
        /// Gets the default type of the wrapper for the view.
        /// </summary>
        private ITypeDescriptor GetDefaultWrapperType()
            => new ResolvedTypeDescriptor(IsMarkupControl() ? typeof(DotvvmMarkupControl) : typeof(DotvvmView));

        private bool IsMarkupControl()
            => fileName.EndsWith(".dotcontrol", StringComparison.Ordinal);

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
