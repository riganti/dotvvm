using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.ViewModel.Serialization
{
    public interface IViewModelLoader
    {

        object InitializeViewModel(IDotvvmRequestContext context, DotvvmView view);

        void DisposeViewModel(object instance);

    }
}