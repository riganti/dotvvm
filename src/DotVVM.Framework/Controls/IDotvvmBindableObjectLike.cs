#nullable enable
using DotVVM.Framework.Binding;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using System;
using System.Collections.Generic;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Interface counterpart of <see cref="DotvvmBindableObject" />.
    /// Since, everything so tightly bound to that specific class, this interface is mostly a marker
    /// </summary>
    public interface IDotvvmBindableObjectLike: IWithDotvvmProperties
    {
        DotvvmBindableObject Self { get; }
    }
}
