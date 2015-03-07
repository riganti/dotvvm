using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
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