using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVM.Framework.Tests
{
    public static class DotvvmTestHelper
    {
        class FakeProtector : IViewModelProtector
        {
            // I hope I will not see this message anywhere on the web ;)
            public const string WarningPrefix = "WARNING: This message should have been encrypted, but you are using a moq IViewModelProtector";
            public static readonly byte[] WarningPrefixBytes = Encoding.UTF8.GetBytes(WarningPrefix);

            public string Protect(string serializedData, IDotvvmRequestContext context)
            {
                return WarningPrefix + ": " + serializedData;
            }

            public byte[] Protect(byte[] plaintextData, params string[] purposes)
            {
                var result = new List<byte>();
                result.AddRange(Encoding.UTF8.GetBytes(WarningPrefix));
                result.AddRange(plaintextData);
                return result.ToArray();
            }

            public string Unprotect(string protectedData, IDotvvmRequestContext context)
            {
                if (!protectedData.StartsWith(WarningPrefix + ": ")) throw new SecurityException($"");
                return protectedData.Remove(0, WarningPrefix.Length + 2);
            }

            public byte[] Unprotect(byte[] protectedData, params string[] purposes)
            {
                if (!protectedData.Take(WarningPrefixBytes.Length).SequenceEqual(WarningPrefixBytes)) throw new SecurityException($"");
                return protectedData.Skip(WarningPrefixBytes.Length).ToArray();
            }
        }

        public static void RegisterMoqServices(IServiceCollection services)
        {
            services.TryAddSingleton<IViewModelProtector, FakeProtector>();
        }

        public static DotvvmConfiguration CreateConfiguration(Action<IServiceCollection> customServices = null) =>
            DotvvmConfiguration.CreateDefault(s => {
                customServices?.Invoke(s);
                RegisterMoqServices(s);
            });
    }
}