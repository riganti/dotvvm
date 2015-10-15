using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense
{
    public class CompletionEqualityComparer : IEqualityComparer<Completion>
    {
        static CompletionEqualityComparer()
        {
            Instance = new CompletionEqualityComparer();
        }

        public static IEqualityComparer<Completion> Instance { get; private set; }

        public bool Equals(Completion x, Completion y)
        {
            return (x == null && y == null)
                || (x != null && x != null && x.DisplayText.Equals(y.DisplayText));
        }

        public int GetHashCode(Completion obj)
        {
            return obj.DisplayText.GetHashCode();
        }
    }
}