using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Hosting
{
    public interface IControlBuilderFactory
    {

        Func<RedwoodControl> GetControlBuilder(MarkupFile markupFile);

    }
}