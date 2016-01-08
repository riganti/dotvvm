using System;
using DotVVM.Framework.Runtime.Compilation.AbstractControlTree;

namespace DotVVM.Framework.Runtime.Compilation.ResolvedControlTree
{
    public struct ResolvedTypeDescriptor : ITypeDescriptor
    {
        private readonly Type type;

        public ResolvedTypeDescriptor(Type type)
        {
            this.type = type;
        }

        public string TypeName => type.Name;
        public string Namespace => type.Namespace;
    }
}