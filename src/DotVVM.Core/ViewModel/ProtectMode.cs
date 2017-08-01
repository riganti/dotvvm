using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotVVM.Framework.ViewModel
{
    /// <summary>
    /// An enumeration of possible view model data protection modes.
    /// </summary>
    public enum ProtectMode
    {
        /// <summary>
        /// The property value is sent to the client unencrypted and it is not signed. It can be modified on the client with no restrictions.
        /// </summary>
        None,

        /// <summary>
        /// The property value is sent to the client unencrypted, but it is also signed. If it is modified on the client, the server will throw an exception during postback.
        /// </summary>
        SignData,

        /// <summary>
        /// The property value is encrypted before it is sent to the client.
        /// </summary>
        EncryptData
    }
}
