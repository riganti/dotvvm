using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.Common.Services;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DataSet
{
    public class GitHubApiStaticCommandsViewModel : DotvvmViewModelBase
    {
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenPagingOptions, NoRowInsertOptions, RowEditOptions> Issues { get; set; } = new(new(), new(), new(), new(), new());
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenHistoryPagingOptions, NoRowInsertOptions, RowEditOptions> Issues2 { get; set; } = new(new(), new(), new(), new(), new());

        public override async Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                await Issues.LoadAsync(LoadIssues);
                await Issues2.LoadAsync(LoadIssues2);
            }

            await base.PreRender();
        }

        [AllowStaticCommand]
        public static async Task<GridViewDataSetResult<IssueDto, NoFilteringOptions, SortingOptions, NextTokenPagingOptions>> LoadIssues(GridViewDataSetOptions<NoFilteringOptions, SortingOptions, NextTokenPagingOptions> options)
        {
            var gitHubService = new GitHubService();

            var result = await gitHubService.GetGitHubIssues(options.PagingOptions.CurrentToken);
            options.PagingOptions.NextPageToken = result.nextToken;

            return new(result.items, options);
        }

        [AllowStaticCommand]
        public static async Task<GridViewDataSetResult<IssueDto, NoFilteringOptions, SortingOptions, NextTokenHistoryPagingOptions>> LoadIssues2(GridViewDataSetOptions<NoFilteringOptions, SortingOptions, NextTokenHistoryPagingOptions> options)
        {
            var gitHubService = new GitHubService();

            var result = await gitHubService.GetGitHubIssues(options.PagingOptions.GetCurrentPageToken());
            options.PagingOptions.SaveNextPageToken(result.nextToken);

            return new(result.items, options);
        }
    }
}

