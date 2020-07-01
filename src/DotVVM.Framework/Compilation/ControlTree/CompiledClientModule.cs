using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public class CompiledClientModule
    {

        public Assembly Assembly { get; set; }

        public Type Type { get; set; }

        public string ResourceName { get; set; }

        public ClientModuleExtensionParameter BindingExtensionParameter { get; set; }

        public List<MemberDeclarationSyntax> Members { get; set; }
    }
}
