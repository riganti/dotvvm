using System;
using System.Linq;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using Microsoft.Owin.Security.DataProtection;

namespace DotVVM.Framework.Security
{
    /// <summary>
    /// Cryptographically protects serialized part of ViewModel with key derived from master key,
    /// with request identity (full URI) and User identity (user name, if authenticated).
    /// </summary>
    public class DefaultViewModelProtector : IViewModelProtector
    {
        private const string PRIMARY_PURPOSE = "DotVVM.Framework.Security.DefaultViewModelProtector";

        private IDataProtectionProvider protectionProvider;

        public DefaultViewModelProtector(IDataProtectionProvider protectionProvider)
        {
            this.protectionProvider = protectionProvider;
        }

        public byte[] Protect(byte[] serializedData, IDotvvmRequestContext context)
        {
            if (serializedData == null) throw new ArgumentNullException(nameof(serializedData));
            if (serializedData.Length == 0) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(serializedData));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.Create(PRIMARY_PURPOSE, userIdentity, requestIdentity);

            // Return protected view model data
            return protector.Protect(serializedData);
        }

        public byte[] Protect(byte[] data, params string[] purposes) =>
            this.protectionProvider
            .Create(ConcatPurposes(PRIMARY_PURPOSE, purposes))
            .Protect(data);

        public byte[] Unprotect(byte[] protectedData, IDotvvmRequestContext context)
        {
            if (protectedData == null) throw new ArgumentNullException(nameof(protectedData));
            if (protectedData.Length == 0) throw new ArgumentException("Value cannot be empty or whitespace only string.", nameof(protectedData));
            if (context == null) throw new ArgumentNullException(nameof(context));

            // Construct protector with purposes
            var userIdentity = ProtectionHelpers.GetUserIdentity(context);
            var requestIdentity = ProtectionHelpers.GetRequestIdentity(context);
            var protector = this.protectionProvider.Create(PRIMARY_PURPOSE, userIdentity, requestIdentity);

            // Return unprotected view model data
            return protector.Unprotect(protectedData);
        }

        public byte[] Unprotect(byte[] protectedData, params string[] purposes) =>
            this.protectionProvider
            .Create(ConcatPurposes(PRIMARY_PURPOSE, purposes))
            .Unprotect(protectedData);

        private string[] ConcatPurposes(string firstPurpose, string[] purposes)
        {
            string[] allPurposes = new string[purposes.Length + 1];
            allPurposes[0] = firstPurpose;
            for (int i = 0; i < purposes.Length; i++)
            {
                allPurposes[i + 1] = purposes[i];
            }

            return allPurposes;
        }

    }
}
