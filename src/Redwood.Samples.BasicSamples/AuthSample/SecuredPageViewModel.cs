using Redwood.Framework.Runtime.Filters;
using Redwood.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Redwood.Samples.BasicSamples.AuthSample
{
    [Authorize]
    public class SecuredPageViewModel : RedwoodViewModelBase
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