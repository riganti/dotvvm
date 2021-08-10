#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.ViewModel.Validation
{
    public interface IViewModelValidator
    {
        IEnumerable<ViewModelValidationError> ValidateViewModel(object? viewModel);
    }
}
