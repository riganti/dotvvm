using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Controls;

namespace Redwood.Framework.Runtime
{
    public interface IControlBuilder
    {

        RedwoodControl BuildControl(IControlBuilderFactory controlBuilderFactory);

    }
}