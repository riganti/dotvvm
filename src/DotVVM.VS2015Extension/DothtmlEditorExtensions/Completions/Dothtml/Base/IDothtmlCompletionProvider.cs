using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public interface IDothtmlCompletionProvider
    {

        TriggerPoint TriggerPoint { get; }

        IEnumerable<SimpleDothtmlCompletion> GetItems(DothtmlCompletionContext context);
        

        void OnWorkspaceChanged();
    }
}