using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redwood.Framework.Hosting;

namespace Redwood.Framework.ViewModel
{
    public interface IRedwoodViewModel
    {

        RedwoodRequestContext Context { get; set; }

        Task Init();

        Task Load();

        Task PreRender();

    }
}
