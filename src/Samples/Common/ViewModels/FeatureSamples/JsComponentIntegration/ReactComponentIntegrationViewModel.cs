using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.JsComponentIntegration
{
    public class ReactComponentIntegrationViewModel : DotvvmViewModelBase
    {
        private Random random = new Random();
        public List<LinearRechartDataDTO> Data { get; set; }
        public ReactComponentIntegrationViewModel()
        {
            Regenerate();
        }

        public void Regenerate()
        {
            Data = Enumerable.Range(0, 15).Select(s =>
                 new LinearRechartDataDTO()
                 {
                     Line1 = 1000 * random.Next(),
                     Line2 = 1000 * random.Next(),
                     Line3 = 1000 * random.Next()
                 }).ToList();
        }

        public bool IncludeInPage { get; set; } = true;
    }

    public class LinearRechartDataDTO
    {
        //{ name: 'Page A', uv: 400, pv: 2400, amt: 0 }, { name: 'Page B', uv: 450, pv: 2800, amt: 2700 }
        public string Name { get; set; }
        public int Line1 { get; set; }
        public int Line2 { get; set; }
        public int Line3 { get; set; }
    }
}

