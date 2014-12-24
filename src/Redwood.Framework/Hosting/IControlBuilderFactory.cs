using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Hosting
{
    public interface IControlBuilderFactory
    {

        Func<IViewCompiler> ViewCompilerFactory { get; }

        IControlBuilder GetControlBuilder(MarkupFile markupFile);

    }
}