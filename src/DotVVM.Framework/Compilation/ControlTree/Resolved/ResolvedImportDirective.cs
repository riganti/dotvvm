using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
    public class ResolvedImportDirective : ResolvedDirective, IAbstractImportDirective
    {
        public string Alias { get; }
        public BindingParserNode NameSyntax { get; }
        public bool HasError { get; }
        public Type Type { get; }

        public bool HasAlias => Alias != null;
        public bool IsNamespace => Type == null && !HasError;
        public bool IsType => Type != null;

        public ResolvedImportDirective(string alias, BindingParserNode nameSyntaxRoot, Type type, bool hasError)
        {
            Alias = alias;
            NameSyntax = nameSyntaxRoot;
            Type = type;
            HasError = hasError;
        }
    }
}
