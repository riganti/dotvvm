using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    public class SecuredPageViewModel : DotvvmViewModelBase
    {
        private static string Message = "server: Hello!";

        public SecuredPageViewModel()
        {
            LastMessage = Message;
        }

        public override Task Init() =>
            Context.Authorize(authenticationSchemes: new [] { "Scheme1" });

        public string MessageEditor { get; set; }

        public string LastMessage { get; set; }

        public async Task ReplaceMessage()
        {
            await Context.Authorize(roles: new [] { "admin" }, authenticationSchemes: new [] { "Scheme1" });
            Message = LastMessage = string.Format("{0}: {1}",
                Context.HttpContext.User.Identity.Name,
                MessageEditor);
        }
    }
}
