using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.DataPager
{
    public class DataPagerViewModel : DotvvmViewModelBase
    {
        public GridViewDataSet<Data> DataSet { get; set; }

        public bool Enabled { get; set; } = true;

        public int ItemsInDatabaseCount { get; set; } = 2;

        public override Task Init()
        {
            DataSet = new GridViewDataSet<Data>()
            {
                Pager =
                {
                    PageSize = 3
                }
            };
            return base.Init();
        }

        public override Task PreRender()
        {
            if (DataSet.IsRefreshRequired)
            {
                DataSet.LoadFromQueryable(FakeDB(ItemsInDatabaseCount));
            }

            return base.PreRender();
        }

        public void Populate()
        {
            ItemsInDatabaseCount = 50;
            DataSet.RequestRefresh();
        }

        private IQueryable<Data> FakeDB(int itemsCreatorCounter)
        {
            var dbdata = new List<Data>();
            for (var i = 0; i < itemsCreatorCounter; i++)
            {
                dbdata.Add(new Data
                {
                    Text = $"Item {i}"
                });
            }
            return dbdata.AsQueryable();
        }

        public class Data
        {
            public string Text { get; set; }
        }
    }
}
