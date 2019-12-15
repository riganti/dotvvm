#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Framework.Runtime
{
    public interface IViewModelParameterBinder
    {

        void BindParameters(IDotvvmRequestContext context, object viewModel);

    }

}
