using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class GithubRepoApiViewModel : DotvvmViewModelBase
    {
        public string UserName { get; set; } = "riganti";
        public string Repo { get; set; } = "dotvvm";

        public string CurrentIssueText { get; set; } = "";

        public GithubApiClient.Issue NewIssue { get; set; } = new GithubApiClient.Issue {
            Labels = new List<string>()
        };
    }
}

