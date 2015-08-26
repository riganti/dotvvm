using System;
using DotVVM.Framework.Parser.Binding.Parser;

namespace DotVVM.Framework.Runtime.Compilation.Binding
{
    public class CompileTimeParserNodeContext : IBindingParserNodeContext
    {

        public DataContextStack DataContextStack { get; set; }

        public Type ActualType { get; set; }

        public CompileTimeTypeConstraint DesiredType { get; set; }
    }
}