using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Compilation;

namespace DotVVM.Framework.Runtime
{
    public interface IControlBuilderFactory
    {
        
        IControlBuilder GetControlBuilder(string virtualPath);

    }
}