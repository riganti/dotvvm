using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample52ViewModel : DotvvmViewModelBase
    {

        public GridViewDataSet<Sample52Data> Data { get; set; } = new GridViewDataSet<Sample52Data>()
        {
            PageSize = 3,
            TotalItemsCount = 2
        };


        public void Populate()
        {
            Data.TotalItemsCount = 20;
        }

        public override Task PreRender()
        {
            Data.Items.Clear();
            for (var i = 0; i < Data.PageSize; i++)
            {
                var number = Data.PageSize * Data.PageIndex + i;
                if (number < Data.TotalItemsCount)
                {
                    Data.Items.Add(new Sample52Data() { Text = "Item " + number });
                }
            }

            return base.PreRender();
        }
    }

    public class Sample52Data
    {
        public string Text { get; set; }
    }
}