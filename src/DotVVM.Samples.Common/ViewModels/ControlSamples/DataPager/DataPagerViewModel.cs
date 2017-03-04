using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.DataPager
{
    public class DataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Data> Data { get; set; } = new GridViewDataSet<Data>
        {
            PagingOptions = new PagingOptions
            {
                PageSize = 3,
                TotalItemsCount = 2
            }
        };

        public bool Enabled { get; set; } = true;


        public void Populate()
        {
            Data.PagingOptions.TotalItemsCount = 20;
        }

        public override Task PreRender()
        {
            Data.Items.Clear();
            for (var i = 0; i < Data.PagingOptions.PageSize; i++)
            {
                var number = Data.PagingOptions.PageSize * Data.PagingOptions.PageIndex + i;
                if (number < Data.PagingOptions.TotalItemsCount)
                {
                    Data.Items.Add(new Data {Text = "Item " + number});
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