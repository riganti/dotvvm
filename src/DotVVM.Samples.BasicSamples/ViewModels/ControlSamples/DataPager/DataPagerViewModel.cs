using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.DataPager
{
    public class DataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Data> Data { get; set; } = new GridViewDataSet<Data>()
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
                    Data.Items.Add(new Data() { Text = "Item " + number });
                }
            }

            return base.PreRender();
        }
    }

    public class Data
    {
        public string Text { get; set; }
    }
}
