using System;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.ResourceManagement
{
    public record DotvvmCyclicResourceDependencyException(
        string ResourceName,
        IResource Resource,
        string[] DependencyChain
    ) : DotvvmExceptionBase(RelatedResource: Resource)
    {
        public override string Message =>
            $"Resource \"{ResourceName}\" has a cyclic dependency: {DependencyChain.StringJoin(" --> ")}";
    }

    internal record DotvvmLinkResourceException(string Message, IResource Resource)
        : DotvvmExceptionBase(Message, RelatedResource: Resource);
}
