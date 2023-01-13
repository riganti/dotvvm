using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization
{
    public class ViewModelTransferredOnlyInPath: DotvvmViewModelBase
    {
        [Bind(Direction.ServerToClientFirstRequest | Direction.ClientToServerInPostbackPath)]
        public List<ViewModel2> Collection { get; set; }

        public string Result { get; set; } = "";

        public override Task PreRender()
        {
            Collection = new() {
                new ViewModel2 { Name = "A" },
                new ViewModel2 { Name = "B" },
                new ViewModel2 { Name = "C" },
                new ViewModel2 { Name = "D" }
            };
            return base.PreRender();
        }

        public void Method(ViewModel2 viewModel2)
        {
            Result = viewModel2.Name;
        }

        public class ViewModel2
        {
            public string Name { get; set; }
            public string Value { get; set; } = "test";
        }
    }
}

