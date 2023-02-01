using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    ///<summary>
    /// Specifies on which requests should the property be serialized and sent. Default is <see cref="Direction.Both">Both</see>.
    /// Set to <see cref="Direction.None">None</see> to disable serialization of the property.
    /// This enumeration can be treated as flags, the directions can be arbitrarily combined.
    ///</summary>
    [Flags]
    public enum Direction
    {
        /// <summary> Never send this property to the client, it won't be allowed to use this property from value and staticCommand bindings. </summary>
        None = 0,
        /// <summary> Sent to client on the initial GET request, but not sent again on postbacks </summary>
        ServerToClientFirstRequest = 1,
        /// <summary> Property is updated on postbacks, but not sent on the first request (initially it will be set to null or default value of the primitive type) </summary>
        ServerToClientPostback = 2,
        /// <summary> Sent from server to client, but not sent back. </summary>
        ServerToClient = ServerToClientFirstRequest | ServerToClientPostback,
        /// <summary> Complement to <see cref="ClientToServerInPostbackPath" />, not meant to be used on its own. </summary>
        ClientToServerNotInPostbackPath = 4,
        /// <summary> Sent from client to server, but only if the current data context is this property. If the data context is a child object of this property, only that part of the object will be sent, all other properties are ignored.
        /// To sent the initial value to client, use <c>Direction.ServerToClientFirstRequest | Direction.ClientToServerInPostbackPath</c> </summary>
        ClientToServerInPostbackPath = 8,
        /// <summary> Sent back on postbacks. Initially the property will set to null or primitive default value. To send the initial value to client, use <c>Direction.ServerToClientFirstRequest | Direction.ClientToServer</c> </summary>
        ClientToServer = ClientToServerInPostbackPath | ClientToServerNotInPostbackPath,
        /// <summary> Always sent to client, sent back only when the object is the current data context (see also <see cref="ClientToServerInPostbackPath"/>) </summary>
        IfInPostbackPath = ServerToClient | ClientToServerInPostbackPath,
        /// <summary> Value is sent on each request. This is the default value. </summary>
        Both = 15,
    }
}
