using System;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Runtime
{
    [Obsolete(DefaultStaticCommandServiceLoader.DeprecationNotice)]
    public interface IStaticCommandServiceLoader
    {
        [Obsolete(DefaultStaticCommandServiceLoader.DeprecationNotice)]
        object GetStaticCommandService(Type serviceType, IDotvvmRequestContext context);

        [Obsolete(DefaultStaticCommandServiceLoader.DeprecationNotice)]
        void DisposeStaticCommandServices(IDotvvmRequestContext context);

    }
}
