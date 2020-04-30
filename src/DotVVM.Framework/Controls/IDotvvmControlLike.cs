#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    public interface IDotvvmControlLike: IRenderable
    {
        DotvvmControl Self { get; }
    }
}
