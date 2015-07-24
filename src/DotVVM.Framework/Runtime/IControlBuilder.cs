using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime
{
    public interface IControlBuilder
    {

        DotvvmControl BuildControl(IControlBuilderFactory controlBuilderFactory);

    }
}