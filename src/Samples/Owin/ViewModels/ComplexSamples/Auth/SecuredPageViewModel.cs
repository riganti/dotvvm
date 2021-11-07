using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    public class SecuredPageViewModel : DotvvmViewModelBase
    {
        private static string Message = "server: Hello!";

        public SecuredPageViewModel()
        {
            LastMessage = Message;
        }
        public override Task Init()
        {
            Context.Authorize();
            return base.Init();
        }

        public string MessageEditor { get; set; }

        public string LastMessage { get; set; }

        public void ReplaceMessage()
        {
            Context.Authorize("admin");
            Message = LastMessage = string.Format("{0}: {1}",
                Context.HttpContext.User.Identity.Name,
                MessageEditor);
        }
    }
}
