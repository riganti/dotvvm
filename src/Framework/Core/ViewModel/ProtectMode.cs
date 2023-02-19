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
        /// The property value is sent to the client unencrypted and it is not signed. It can be modified on the client with no restrictions. This is the default.
        /// </summary>
        None,

        /// <summary>
        /// The property value is sent to the client in both unencrypted and encrypted form. On server, the encrypted value is read, so it cannot be modified on the client.
        /// </summary>
        SignData,

        /// <summary>
        /// The property value is encrypted before it is sent to the client. Encrypted properties thus cannot be used in value bindings.
        /// </summary>
        EncryptData
    }
}
