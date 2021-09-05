using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ButtonInMarkupControl
{
    public class EnabledViewModel
    {
        public bool Enabled { get; set; }

        public TestDto Dto { get; set; } = new TestDto();

        public Task Flip()
        {
            Enabled = !Enabled;
            return TaskUtils.GetCompletedTask();
        }

        public class TestDto
        {
            public string Label { get; set; } = "Hello";
        }
    }
}
