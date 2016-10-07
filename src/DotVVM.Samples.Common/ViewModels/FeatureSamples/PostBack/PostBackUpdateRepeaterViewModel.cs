using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class PostBackUpdateRepeaterViewModel : DotvvmViewModelBase
    {

        public string Value { get; set; }

        public List<string> Items { get; set; }


        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Items = new List<string>()
                {
                    Value,
                    Value,
                    Value,
                    Value,
                    Value
                };
            }
            return base.Init();
        }


        public void Apply()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                Items[i] = Value;
            }
        }
    }
}