using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Api.Common.Model;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class GridViewDataSetOwinViewModel : DotvvmViewModelBase
    {

        public SortingOptions SortingOptions1 { get; set; } = new SortingOptions()
        {
            SortExpression = nameof(Company<string>.Id)
        };

        [Bind(Direction.ServerToClientFirstRequest)]
        public GridViewDataSet<Company<string>> DataSet1 { get; set; } = new GridViewDataSet<Company<string>>()
        {
            SortingOptions =
            {
                SortExpression = nameof(Company<string>.Id)
            },
            PagingOptions =
            {
                PageSize = 10
            },
            Items = new List<Company<string>>()
        };

        [Bind(Direction.ServerToClientFirstRequest)]
        public GridViewDataSet<Company<string>> DataSet2 { get; set; } = new GridViewDataSet<Company<string>>()
        {
            SortingOptions =
            {
                SortExpression = nameof(Company<string>.Id)
            },
            PagingOptions =
            {
                PageSize = 10
            }
        };
    }
}

