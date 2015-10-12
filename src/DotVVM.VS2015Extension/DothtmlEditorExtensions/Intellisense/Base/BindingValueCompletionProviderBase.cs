using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public abstract class BindingValueCompletionProviderBase : DothtmlCompletionProviderBase
    {
        public override TriggerPoint TriggerPoint
        {
            get { return TriggerPoint.BindingValue; }
        }
    }
}