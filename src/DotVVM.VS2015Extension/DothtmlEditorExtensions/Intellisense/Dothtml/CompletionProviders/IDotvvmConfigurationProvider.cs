using DotVVM.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Intellisense.Dothtml.CompletionProviders
{
    public interface IDotvvmConfigurationProvider
    {
        event EventHandler ConfigurationChanged;

        DotvvmConfiguration GetConfiguration();
    }
}