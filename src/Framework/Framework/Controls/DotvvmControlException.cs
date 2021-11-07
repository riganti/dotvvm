using System;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;

namespace DotVVM.Framework.Controls
{
    public record DotvvmControlException : DotvvmExceptionBase
    {
        public string? FileName => Location?.FileName;

        public DotvvmControlException(DotvvmBindableObject control, string message, Exception? innerException = null)
            : base(
            message,
            RelatedControl: control,
            InnerException: innerException
        )
        {
        }

        public DotvvmControlException(
            string message,
            Exception? innerException = null,
            DotvvmLocationInfo? location = null)
            : base(message, Location: location, InnerException: innerException)
        {
        }
    }
}
