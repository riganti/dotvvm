using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml
{
    public interface IDotvvmConfigurationProvider
    {
        DotvvmConfiguration GetConfiguration();

        event EventHandler ConfigurationChanged;
    }
}