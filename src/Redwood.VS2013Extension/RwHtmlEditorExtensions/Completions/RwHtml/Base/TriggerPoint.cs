using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.VS2013Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base
{
    public enum TriggerPoint
    {
        DirectiveName,
        DirectiveValue,
        TagName,
        TagAttributeName,
        TagAttributeValue,
        BindingName,
        BindingValue
    }
}