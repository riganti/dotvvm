using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    [Authorize(AuthScheme = "Scheme1")]
    public class SecuredPageViewModel : DotvvmViewModelBase
    {
        private static string Message = "server: Hello!";

        public string MessageEditor { get; set; }

        public string LastMessage { get; set; }

        public SecuredPageViewModel()
        {
            LastMessage = Message;
        }

        [Authorize("admin", AuthScheme = "Scheme1")]
        public void ReplaceMessage()
        {
            Message = LastMessage = string.Format("{0}: {1}",
                Context.HttpContext.User.Identity.Name,
                MessageEditor);
        }
    }
}
