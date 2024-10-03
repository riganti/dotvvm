using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.Common.Services;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DataSet
{
    public class GitHubApiViewModel : DotvvmViewModelBase
    {
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenPagingOptions, NoRowInsertOptions, RowEditOptions> Issues { get; set; } = new(new(), new(), new(), new(), new());
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenHistoryPagingOptions, NoRowInsertOptions, RowEditOptions> Issues2 { get; set; } = new(new(), new(), new(), new(), new());


        public override async Task PreRender()
        {
            var gitHubService = new GitHubService();

            if (Issues.IsRefreshRequired)
            {
                var result = await gitHubService.GetGitHubIssues(Issues.PagingOptions.CurrentToken);
                Issues.Items = result.items;
                Issues.PagingOptions.NextPageToken = result.nextToken;
            }

            if (Issues2.IsRefreshRequired)
            {
                var result = await gitHubService.GetGitHubIssues(Issues2.PagingOptions.GetCurrentPageToken());
                Issues2.Items = result.items;
                Issues2.PagingOptions.SaveNextPageToken(result.nextToken);
            }

            await base.PreRender();
        }
    }

}

