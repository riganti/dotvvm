using System;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using FastExpressionCompiler;

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
        string? Msg = null,
        DotvvmProperty? RelatedProperty = null,
        DotvvmBindableObject? RelatedControl = null,
        IBinding? RelatedBinding = null,
        ResolvedTreeNode? RelatedResolvedControl = null,
        DothtmlNode? RelatedDothtmlNode = null,
        IResource? RelatedResource = null,
        DotvvmLocationInfo? Location = null,
        Exception? InnerException = null
    ) : RecordExceptions.RecordException(Msg, InnerException), IDotvvmException
    {
        // small hack so that we can automatically set this property in InvokePageLifeCycleEvent
        public DotvvmBindableObject? RelatedControl { get; set; } = RelatedControl;
        public DotvvmLocationInfo? Location { get; set; } = Location;
        Exception IDotvvmException.TheException => this;

        protected override bool PrintMembers(StringBuilder builder)
        {
            if (base.PrintMembers(builder))
                builder.Append(", ");

            if (Location is {})
            {
                var str = Location switch {
                    { FileName: {} file, LineNumber: {} line } => $"{file}:{line}",
                    { FileName: {} file } => file,
                    { LineNumber: {} line } => $"Line {line}",
                    _ => null
                };
                if (str != null)
                    builder.Append("Location = ").Append(str).Append(", ");
                if (Location.ControlType is {} && RelatedControl is null)
                    builder.Append("ControlType = ").Append(Location.ControlType.ToCode(stripNamespace: true)).Append(", ");
            }

            if (RelatedProperty is {})
                builder.Append("Property = ").Append(RelatedProperty.Name).Append(", ");

            if (RelatedControl is {})
                builder.Append("Control = ").Append(RelatedControl.DebugString(multiline: false)).Append(", ");

            if (RelatedBinding is {})
                builder.Append("Binding = ").Append(RelatedBinding.ToString()).Append(", ");

            if (RelatedResolvedControl is {})
                builder.Append("ResolvedControl = ").Append(RelatedResolvedControl.ToString()).Append(", ");

            if (RelatedDothtmlNode is {})
                builder.Append("DothtmlNode = ").Append(RelatedDothtmlNode.ToString()).Append(", ");

            if (RelatedResource is {})
                builder.Append("Resource = ").Append(RelatedResource).Append(", ");

            return false;
        }
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
