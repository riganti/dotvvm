using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redwood.Framework.ViewModel
{
    public enum CryptoSettings
    {
        None,
        /// <summary>
        /// read only for client, symetric signature
        /// </summary>
        Mac,
        /// <summary>
        /// no access on client, symetric encryption
        /// </summary>
        AuthenticatedEncrypt
    }
}
