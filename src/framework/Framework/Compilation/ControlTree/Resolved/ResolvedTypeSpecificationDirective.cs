using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public abstract class ResolvedTypeSpecificationDirective : ResolvedDirective, IAbstractTypeSpecificationDirective
    {
        public BindingParserNode NameSyntax { get; }
        public ResolvedTypeDescriptor ResolvedType { get; }

        ITypeDescriptor IAbstractTypeSpecificationDirective.ResolvedType => ResolvedType;

        public ResolvedTypeSpecificationDirective(BindingParserNode nameSyntax, ResolvedTypeDescriptor resolvedType)
        {
            this.NameSyntax = nameSyntax;
            this.ResolvedType = resolvedType;
        }
    }
    public sealed class ResolvedViewModelDirective : ResolvedTypeSpecificationDirective, IAbstractViewModelDirective
    {
        public ResolvedViewModelDirective(BindingParserNode nameSyntax, ResolvedTypeDescriptor resolvedType)
            : base(nameSyntax, resolvedType) { }
    }
    public sealed class ResolvedBaseTypeDirective : ResolvedTypeSpecificationDirective, IAbstractBaseTypeDirective
    {
        public ResolvedBaseTypeDirective(BindingParserNode nameSyntax, ResolvedTypeDescriptor resolvedType)
            : base(nameSyntax, resolvedType) { }
    }
}
