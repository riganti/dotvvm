using System;
using System.Collections.Generic;
using System.Linq;
using Redwood.Framework.Hosting;
using Redwood.Framework.Runtime.Compilation;

namespace Redwood.Framework.Runtime
{
    public interface IControlBuilderFactory
    {
        
        IControlBuilder GetControlBuilder(string virtualPath);

    }
}