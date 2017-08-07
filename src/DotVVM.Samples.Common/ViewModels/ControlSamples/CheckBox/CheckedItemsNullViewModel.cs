using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class CheckedItemsNullViewModel : WithColorsViewModel
    {
        public override Task PreRender()
        {
            Colors = null;
            return base.PreRender();
        }

    }
}

