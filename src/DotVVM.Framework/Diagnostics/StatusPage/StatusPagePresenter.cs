using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Security;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Diagnostics.StatusPage
{
    internal class StatusPagePresenter : DotvvmPresenter
    {
        public StatusPagePresenter(DotvvmConfiguration configuration, IDotvvmViewBuilder viewBuilder, IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer, IOutputRenderer outputRender, ICsrfProtector csrfProtector,
            IViewModelParameterBinder viewModelParameterBinder, IStaticCommandServiceLoader staticCommandServiceLoader) :
            base(configuration, viewBuilder, new DefaultViewModelLoader(), viewModelSerializer, outputRender, csrfProtector, viewModelParameterBinder, staticCommandServiceLoader)
        {
        }
    }
}
