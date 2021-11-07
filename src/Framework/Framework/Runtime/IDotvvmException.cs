using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    /// <summary> Common interface for exceptions that are related to some DotVVM objects - controls, bindings or properties </summary>
    public interface IDotvvmException
    {
        /// <summary> Returns itself. This is a workaround for the interface inheriting from <see cref="System.Exception" /> </summary>
        Exception TheException { get; }

        DotvvmProperty? RelatedProperty { get; }
        DotvvmBindableObject? RelatedControl { get; }
        IBinding? RelatedBinding { get; }
        ResolvedTreeNode? RelatedResolvedControl { get; }
        DothtmlNode? RelatedDothtmlNode { get; }
        IResource? RelatedResource { get; }
        DotvvmLocationInfo? Location { get; }
    }

    public abstract record DotvvmExceptionBase(
        string? msg = null,
        DotvvmProperty? RelatedProperty = null,
        DotvvmBindableObject? RelatedControl = null,
        IBinding? RelatedBinding = null,
        ResolvedTreeNode? RelatedResolvedControl = null,
        DothtmlNode? RelatedDothtmlNode = null,
        IResource? RelatedResource = null,
        DotvvmLocationInfo? Location = null,
        Exception? InnerException = null
    ) : RecordExceptions.RecordException(msg, InnerException), IDotvvmException
    {
        // small hack so that we can automatically set this property in InvokePageLifeCycleEvent
        public DotvvmBindableObject? RelatedControl { get; set; } = RelatedControl;
        public DotvvmLocationInfo? Location { get; set; } = Location;
        Exception IDotvvmException.TheException => this;
    }

    public static class DotvvmExceptionExtensions
    {
        public static DotvvmLocationInfo? GetLocation(this IDotvvmException e)
        {
            return e.Location ??
                   e.RelatedBinding?.GetProperty<DotvvmLocationInfo>(ErrorHandlingMode.ReturnNull) ??
                   e.RelatedControl?.Apply(DotvvmLocationInfo.FromControl);
        }
    }
}
