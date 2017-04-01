using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DataSource
{
	public class SimpleSqlDataSourceViewModel : DotvvmViewModelBase
	{
        public string SelectedAuthor { get; set; }

        public void SelectAuthor(object name)
        {
            SelectedAuthor = (string)name;
        }
    }
}

