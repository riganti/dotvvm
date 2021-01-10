using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.RadioButton
{
    public class RadioButton_ObjectsViewModel : DotvvmViewModelBase
    {

        public ColorData SelectedColor { get; set; } = new ColorData() { Id = 1, Name = "Red" };

        public List<ColorData> Colors { get; set; } = new List<ColorData>()
        {
            new ColorData() { Id = 1, Name = "Red" },
            new ColorData() { Id = 2, Name = "Green" },
            new ColorData() { Id = 3, Name = "Blue" }
        };


        public void SetSelection()
        {
            SelectedColor = new ColorData() { Id = 2, Name = "Green" };
        }

        public class ColorData
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }


    }
}

