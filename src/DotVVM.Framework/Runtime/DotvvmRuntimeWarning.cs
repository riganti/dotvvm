using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Runtime
{
    public class DotvvmRuntimeWarning
    {
        public DotvvmRuntimeWarning(string message, Exception relatedException = null, DotvvmBindableObject relatedControl = null)
        {
            this.Message = message ?? throw new ArgumentNullException(nameof(message));
            this.RelatedException = relatedException;
            this.RelatedControl = relatedControl;
        }

        public string Message { get; }
        public Exception RelatedException { get; }
        public DotvvmBindableObject RelatedControl { get; }

        public override string ToString()
        {
            var sections = new string[] {
                Message,
                RelatedControl?.Apply(c => "related to:\n" + c.DebugString()),
                RelatedException?.ToString(),
            };
            return string.Join("\n\n", sections.Where(s => s is object));
        }
    }
}
