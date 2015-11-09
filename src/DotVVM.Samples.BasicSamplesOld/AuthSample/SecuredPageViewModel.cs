using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DotVVM.Samples.BasicSamples.AuthSample
{
    [Authorize]
    public class SecuredPageViewModel : DotvvmViewModelBase
    {
        private static string Message = "server: Hello!";

        public string MessageEditor { get; set; }

        public string LastMessage { get; set; }

        public SecuredPageViewModel()
        {
            LastMessage = Message;
        }

        [Authorize("admin")]
        public void ReplaceMessage()
        {
            Message = LastMessage = string.Format("{0}: {1}",
                Context.OwinContext.Authentication.User.Identity.Name,
                MessageEditor);
        }
    }
}