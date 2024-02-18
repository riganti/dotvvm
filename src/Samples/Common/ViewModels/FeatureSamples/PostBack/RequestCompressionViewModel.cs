using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class RequestCompressionViewModel : DotvvmViewModelBase
    {
        public string LargeField { get; set; } = new string('a', 100_000);

        [Bind(Direction.ServerToClientFirstRequest)]
        public int[] RequestSizes { get; set; } = new int[0];
        [Bind(Direction.ServerToClientFirstRequest)]
        public int[] ResponseSizes { get; set; } = new int[0];

        public void Command()
        {
            LargeField += "b";
        }

        [AllowStaticCommand]
        public static string StaticCommand(string data)
        {
            return data + "c";
        }
    }
}
