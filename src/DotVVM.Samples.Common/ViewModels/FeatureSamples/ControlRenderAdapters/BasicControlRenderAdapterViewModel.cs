using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.ControlRenderAdapters
{
    public class BasicControlRenderAdapterViewModel : DotvvmViewModelBase
    {
        public BasicControlRenderAdapterViewModel()
        {
            Context.Configuration.ExperimentalFeatures.ControlRenderAdapters.Enabled = true;
        }

        public override Task Init()
        {
            var literal = Context.View.FindControlByClientId<Literal>("replaced");
            literal.SetValue(Internal.RenderAdapterProperty, new TestControlRenderAdapter());
            return base.Init();
        }
    }
}

