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
        }

        public override Task Init()
        {
            Context.Configuration.ExperimentalFeatures.ControlRenderAdapters.Enabled = true;

            return base.Init();
        }

        public override Task PreRender()
        {

            var literal = Context.View.FindControlByClientId<TextBox>("replaced");
            literal.SetValueRaw(Internal.RenderAdapterProperty, new TestControlRenderAdapter());
            return base.PreRender();
        }


    }
}

