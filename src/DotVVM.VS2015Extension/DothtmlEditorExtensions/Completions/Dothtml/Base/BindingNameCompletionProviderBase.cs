using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base
{
    public abstract class BindingNameCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.BindingName; }
        }
        
    }
}