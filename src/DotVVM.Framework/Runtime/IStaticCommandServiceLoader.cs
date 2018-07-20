using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
{
    public interface IStaticCommandServiceLoader
    {

        object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context);

    }
}
