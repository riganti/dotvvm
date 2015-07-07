using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions
{
    
    public class CustomCompletionSet : CompletionSet
    {
        public CustomCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders) 
            : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
        }

        public override void Filter()
        {
            FixTrackingSpan();
            base.Filter();
        }

        public override void Recalculate()
        {
            FixTrackingSpan();
            base.Recalculate();
        }

        public override void SelectBestMatch()
        {
            FixTrackingSpan();
            base.SelectBestMatch();
        }

        private void FixTrackingSpan()
        {
            var currentSnapshot = ApplicableTo.TextBuffer.CurrentSnapshot;
            var text = ApplicableTo.GetText(currentSnapshot);
            if (text.EndsWith("}"))
            {
                ApplicableTo = currentSnapshot.CreateTrackingSpan(ApplicableTo.GetStartPoint(currentSnapshot).Position, text.Length - 1, SpanTrackingMode.EdgeInclusive);
            }
        }
    }
}