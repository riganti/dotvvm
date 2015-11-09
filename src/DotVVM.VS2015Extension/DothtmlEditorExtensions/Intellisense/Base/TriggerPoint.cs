using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Base
{
    public enum TriggerPoint
    {
        None,
        DirectiveName,
        DirectiveValue,
        TagName,
        TagAttributeName,
        TagAttributeValue,
        BindingName,
        BindingValue
    }
}