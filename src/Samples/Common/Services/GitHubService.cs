using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotVVM.Samples.BasicSamples;

namespace DotVVM.Samples.Common.Services;

public class GitHubService
{

    public async Task<(IssueDto[] items, string nextToken)> GetGitHubIssues(string currentToken)
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
