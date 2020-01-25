using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Api
{
    public class GithubRepoApiViewModel : DotvvmViewModelBase
    {
        public string UserName { get; set; } = "riganti";
        public string Repo { get; set; } = "dotvvm";
        public string Token => SampleConfiguration.Instance.AppSettings["githubApiToken"];

        public string CurrentIssueText { get; set; } = "";

        public GithubApiClient.Issue NewIssue { get; set; } = new GithubApiClient.Issue {
            Labels = new List<string>()
        };


        public override Task PreRender()
        {
            var script = $@"function githubAuthenticatedFetch(input, init) {{
                if (dotvvm.viewModels.root.viewModel.Token()) {{
                    init.headers.append('Authorization', 'token ' + dotvvm.viewModels.root.viewModel.Token());
                }}
                return window.fetch(input, init);
            }}";
            Context.ResourceManager.AddStartupScript(script);

            return base.PreRender();
        }
    }
}

