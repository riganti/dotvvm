using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Security
{
    /// <summary>
    /// Cryptographically protects serialized part of ViewModel with key derived from master key,
    /// with request identity (full URI) and User identity (user name, if authenticated).
    /// </summary>
    public class DefaultViewModelProtector : IViewModelProtector
    {
        private const string KDF_LABEL = "DotVVM.Framework.Security.DefaultViewModelProtector";

        public string Protect(string serializedData, DotvvmRequestContext context)
        {
            if (serializedData == null) throw new ArgumentNullException("serializedData");
            if (string.IsNullOrWhiteSpace(serializedData)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "serializedData");
            if (context == null) throw new ArgumentNullException("context");

            // Get application key helper
            var keyHelper = new ApplicationKeyHelper(context.Configuration.Security);

            // Protect serialized data
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            return keyHelper.ProtectString(serializedData, KDF_LABEL, userIdentity, requestIdentity);
        }

        public string Unprotect(string protectedData, DotvvmRequestContext context)
        {
            if (protectedData == null) throw new ArgumentNullException("protectedData");
            if (string.IsNullOrWhiteSpace(protectedData)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "protectedData");
            if (context == null) throw new ArgumentNullException("context");

            // Get application key helper
            var keyHelper = new ApplicationKeyHelper(context.Configuration.Security);

            // Unprotect serialized data
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            return keyHelper.UnprotectString(protectedData, KDF_LABEL, userIdentity, requestIdentity);
        }

    }
}
