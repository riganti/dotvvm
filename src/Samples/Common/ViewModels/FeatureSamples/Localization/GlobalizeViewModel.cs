using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Localization
{
    public class GlobalizeViewModel : DotvvmViewModelBase
    {
        public int PostBackCounter { get; set; }

        public string SayHelloText { get; set; }

        public int? MultiplyResult { get; set; }

        [Bind(Direction.ClientToServer)]
        public int MultiplyInputA { get; set; }

        [Bind(Direction.ClientToServer)]
        public int MultiplyInputB { get; set; }

        public int? ParseResult { get; set; }

        [Bind(Direction.ClientToServer)]
        public string ParseTextInput { get; set; }

        public override Task Load()
        {
            if (Context.IsPostBack)
            {
                PostBackCounter++;
            }
            return base.Load();
        }

        public void SayHello()
        {
            SayHelloText = "Hello";
        }

        public void Multiply(int a, int b)
        {
            MultiplyResult = a * b;
        }

        public void Parse(string str)
        {
            ParseResult = int.Parse(str);
        }
    }
}
