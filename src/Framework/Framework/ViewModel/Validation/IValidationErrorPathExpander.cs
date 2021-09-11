using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IValidationErrorPathExpander
    {
        void Expand(ModelState modelState, object? viewModel);
    }
}
