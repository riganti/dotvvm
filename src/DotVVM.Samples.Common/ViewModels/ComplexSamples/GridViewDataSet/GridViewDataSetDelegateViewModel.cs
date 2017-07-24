using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;


namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.GridViewDataSet
{
    public class GridViewDataSetDelegateViewModel : DotvvmViewModelBase
    {

        public int CallDelegateCounter { get; set; } = 0;

        public override Task Init()
        {
            DataSet = Framework.Controls.GridViewDataSet.Create((options) => Task.FromResult(GetData(options)), pageSize: 3);
            return base.Init();
        }

        public GridViewDataSet<Data> DataSet { get; set; } 

        public int ItemsCount { get; set; } = 20;

        private GridViewDataSetLoadedData<Data> GetData(IGridViewDataSetLoadOptions gridViewDataSetLoadOptions)
        {
            CallDelegateCounter++;

            var queryable = TestDB(ItemsCount);
            return queryable.GetDataFromQueryable(gridViewDataSetLoadOptions);
        }
      
        private IQueryable<Data> TestDB(int itemsCreatorCounter)
        {
            var dbdata = new List<Data>();
            for (var i = 0; i < itemsCreatorCounter; i++)
            {
                dbdata.Add(new Data
                {
                    Id = i,
                    Text = $"Item {i}"
                });
            }
            return dbdata.AsQueryable();
        }
    }
    public class Data
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }


}