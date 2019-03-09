using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Api.Common.Model;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class GridViewDataSetAspNetCoreViewModel : DotvvmViewModelBase
    {

        public SortingOptions SortingOptions1 { get; set; } = new SortingOptions() {
            SortExpression = nameof(Company<string>.Id)
        };

        public GridViewDataSet<Company<string>> DataSet1 { get; set; } = new GridViewDataSet<Company<string>>() {
            SortingOptions =
            {
                SortExpression = nameof(Company<string>.Id)
            },
            PagingOptions =
            {
                PageSize = 10
            }
        };

        public GridViewDataSet<Company<string>> DataSet2 { get; set; } = new GridViewDataSet<Company<string>>() {
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
