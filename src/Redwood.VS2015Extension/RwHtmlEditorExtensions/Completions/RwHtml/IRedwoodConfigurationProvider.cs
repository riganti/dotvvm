using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Configuration;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml
{
    public interface IRedwoodConfigurationProvider
    {
        RedwoodConfiguration GetConfiguration();

        event EventHandler ConfigurationChanged;
    }
}