using DotVVM.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample53ViewModel: DotvvmViewModelBase
    {
        [Bind(Direction.ServerToClient)]
        public string ServerToClient { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public string ServerToClientFirstRequest { get; set; }
        [Bind(Direction.ServerToClientPostback)]
        public string ServerToClientPostback { get; set; }
        [Bind(Direction.ClientToServer)]
        public string ClientToServer { get; set; } = "blabla";
        public string ClientToServerMirror { get; set; }

        public override Task Load()
        {
            if (ServerToClient != null || ServerToClientFirstRequest != null || ServerToClientPostback != null) throw new Exception("client sends some data it shouldn't");

            ServerToClient = Guid.NewGuid().ToString();
            ServerToClientFirstRequest = Guid.NewGuid().ToString();
            ServerToClientPostback = Guid.NewGuid().ToString();
            
            ClientToServerMirror = ClientToServer;
            ClientToServer = Guid.NewGuid().ToString();

            return base.Load();
        }

        public void Postback() { }
    }
}