using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    ///<summary>
    /// ServerToClient, ServerToClient on postback, ClientToServer, C2S iff in command path
    ///</summary>
    [Flags]
    public enum Direction
    {
        None = 0,
        ServerToClientFirstRequest = 1,
        ServerToClientPostback = 2,
        ServerToClient = ServerToClientFirstRequest | ServerToClientPostback,
        ClientToServerNotInPostbackPath = 4,
        ClientToServerInPostbackPath = 8,
        ClientToServer = ClientToServerInPostbackPath | ClientToServerNotInPostbackPath,
        IfInPostbackPath = ServerToClient | ClientToServerInPostbackPath,
        Both = 15,
    }
}