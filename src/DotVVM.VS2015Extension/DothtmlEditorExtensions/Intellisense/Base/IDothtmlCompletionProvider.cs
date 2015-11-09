using DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.Completions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public interface IDothtmlCompletionProvider
    {
        TriggerPoint TriggerPoint { get; }

        IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context);

        void OnWorkspaceChanged();
    }
}