using System;

namespace DotVVM.Framework.Testing
{
    public class TestEnvironmentHelper
    {
        public enum FrameworkType
        {
            /// Represents .NET 5+
            Net,

            /// Represents .NET Core 1.0 - 3.1
            NetCore,

            /// Represents old .NET Framework 4.7.1 - 4.8
            NetFramework,

            /// Represents .NET Native
            NetNative
        }

        public static FrameworkType GetFrameworkType()
        {
            var description = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
            if (description.StartsWith(".NET Framework"))
                return FrameworkType.NetFramework;
            else if (description.StartsWith(".NET Core"))
                return FrameworkType.NetCore;
            else if (description.StartsWith(".NET Native"))
                return FrameworkType.NetNative;
            else if (description.StartsWith(".NET"))
                return FrameworkType.Net;

            throw new ArgumentException($"Unrecognized framework type {description}");
        }
    }
}
