using System;
using System.Runtime.InteropServices.JavaScript;

if (!OperatingSystem.IsBrowser())
{
    throw new PlatformNotSupportedException("This application is expected to run on browser platform.");
}

//await JSHost.ImportAsync("dotvvm-interop-dotnet", "/wasm/main.js");
