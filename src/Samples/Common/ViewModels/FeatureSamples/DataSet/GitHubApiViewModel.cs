using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Samples.BasicSamples;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.DataSet
{
    public class GitHubApiViewModel : DotvvmViewModelBase
    {
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenPagingOptions, NoRowInsertOptions, RowEditOptions> Issues { get; set; } = new(new(), new(), new(), new(), new());
        public GenericGridViewDataSet<IssueDto, NoFilteringOptions, SortingOptions, NextTokenHistoryPagingOptions, NoRowInsertOptions, RowEditOptions> Issues2 { get; set; } = new(new(), new(), new(), new(), new());


        public override async Task PreRender()
        {
            if (Issues.IsRefreshRequired)
            {
                var result = await GetGitHubIssues(Issues.PagingOptions.CurrentToken);
                Issues.Items = result.items;
                Issues.PagingOptions.NextPageToken = result.nextToken;
            }

            if (Issues2.IsRefreshRequired)
            {
                var result = await GetGitHubIssues(Issues2.PagingOptions.GetCurrentPageToken());
                Issues2.Items = result.items;
                Issues2.PagingOptions.SaveNextPageToken(result.nextToken);
            }

            await base.PreRender();
        }

        private async Task<(IssueDto[] items, string nextToken)> GetGitHubIssues(string currentToken)
        {
            var client = GetGitHubClient();

            var response = await client.GetAsync(currentToken ?? "https://api.github.com/repos/riganti/dotvvm/issues");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<IssueDto[]>();

            return (items, ParseNextToken(response));
        }

        private static HttpClient GetGitHubClient()
        {
            var client = new HttpClient();
            var token = SampleConfiguration.Instance.AppSettings[DotvvmStartup.GitHubTokenConfigName];
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            client.DefaultRequestHeaders.Add("User-Agent", "tomasherceg");
            return client;
        }

        private string ParseNextToken(HttpResponseMessage response)
        {
            var linkHeader = response.Headers.GetValues("Link").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(linkHeader))
            {
                return null;
            }

            var match = Regex.Match(linkHeader, @"<([^>]+)>; rel=""next""");
            return match.Success ? match.Groups[1].Value : null;
        }
    }

    public class IssueDto
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public string Title { get; set; }

        public string State { get; set; }
    }
}

