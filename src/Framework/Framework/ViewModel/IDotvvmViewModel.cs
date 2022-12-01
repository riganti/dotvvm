using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel
{
    public interface IDotvvmViewModel
    {
        IDotvvmRequestContext Context { get; set; }

        /// <summary> Initializes the view model. Called by DotVVM before incoming JSON viewmodel is deserialized. </summary>
        Task Init();

        /// <summary> Loads additional data into the view model. Called by DotVVM after viewmodel is deserialized and before command is invoked. </summary>
        Task Load();

        /// <summary> Called by DotVVM after the command is invoked and before output is rendered. Useful for re-loading data after the command is invoked (for example, after new page index is changed). </summary>
        Task PreRender();

    }
}
