using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ValidationSummary
{
    public class PerformanceViewModel : DotvvmViewModelBase
    {
        public const int TreeWidth = 3;
        public const int TreeDepth = 5;
        public const string ValidText = "Hello";
        public const string InvalidText = null;
        public const int ValidNumber = 11;
        public const int InvalidNumber = -1;

        public Data CreatedData { get; set; }

        public override Task Load()
        {
            CreatedData = CreateData(TreeDepth);
            return base.Load();
        }

        private Data CreateData(int depth)
        {
            var data = new Data();
            if (depth <= 0)
            {
                return data;
            }

            for (int i = 0; i < TreeWidth; i++)
            {
                var child = CreateData(depth - 1);
                child.SomeText = i % 3 == 0
                    ? InvalidText
                    : ValidText;
                child.SomeNumber = i % 5 == 0
                    ? InvalidNumber
                    : ValidNumber;
                data.Children.Add(child);
            }
            return data;
        }

        public class Data
        {
            [Required]
            public string SomeText { get; set; } = "Default";

            [Required]
            [Range(0, 42)]
            public int SomeNumber { get; set; } = 42;

            public List<Data> Children { get; set; } = new List<Data>();
        }
    }
}
