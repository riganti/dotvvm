using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    public class CompletionEqualityComparer : IEqualityComparer<Completion>
    {
        public static IEqualityComparer<Completion> Instance { get; private set; }

        static CompletionEqualityComparer()
        {
            Instance = new CompletionEqualityComparer();
        }

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