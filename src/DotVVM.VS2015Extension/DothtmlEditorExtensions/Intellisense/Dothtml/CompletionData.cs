using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml
{
    public class CompletionData
    {
        public CompletionData(string text) : this(text, text)
        {
        }

        public CompletionData(string displayText, string completionText)
        {
            DisplayText = displayText;
            CompletionText = completionText;
        }

        public string DisplayText { get; private set; }

        public string CompletionText { get; private set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((DisplayText != null ? DisplayText.GetHashCode() : 0) * 397) ^ (CompletionText != null ? CompletionText.GetHashCode() : 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((CompletionData)obj);
        }

        protected bool Equals(CompletionData other)
        {
            return string.Equals(DisplayText, other.DisplayText) && string.Equals(CompletionText, other.CompletionText);
        }
    }
}