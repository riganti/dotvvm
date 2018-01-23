using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Api.Owin;
using SortingOptions = DotVVM.Framework.Controls.SortingOptions;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class GridViewDataSetOwinViewModel : DotvvmViewModelBase
    {

        public SortingOptions SortingOptions1 { get; set; } = new SortingOptions()
        {
            SortExpression = nameof(Company.Id)
        };

        [Bind(Direction.ServerToClientFirstRequest)]
        public GridViewDataSet<Company> DataSet1 { get; set; } = new GridViewDataSet<Company>()
        {
            SortingOptions =
            {
                SortExpression = nameof(Company.Id)
            },
            PagingOptions =
            {
                PageSize = 10
            }
        };

        [Bind(Direction.ServerToClientFirstRequest)]
        public GridViewDataSet<Company> DataSet2 { get; set; } = new GridViewDataSet<Company>()
        {
            SortingOptions =
            {
                SortExpression = nameof(Company.Id)
            },
            PagingOptions =
            {
                PageSize = 10
            }
        };
    }
}

