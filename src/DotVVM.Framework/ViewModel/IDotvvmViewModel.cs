using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel
{
    public interface IDotvvmViewModel
    {

        DotvvmRequestContext Context { get; set; }

        Task Init();

        Task Load();

        Task PreRender();

    }
}
