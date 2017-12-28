using System;

namespace DotVVM.Compiler
{
    /// <summary>
    /// References versions MUST match with reference versions on Dotvvm.Framework, or else compiler will not be able to load them.
    /// Project that will use Dotvvm will have to use at least the version stated in Dotvvm nuget.
    /// However if versions here are lower, there is no way to ensure them
    /// </summary>
    internal static class Program
    {

        private static void Main(string[] args)
        {
            if (!AppDomain.CurrentDomain.ShadowCopyFiles)
            {
                var appDomain = AppDomain.CreateDomain("SecondaryDomainShadowCopyAllowed", null, new AppDomainSetup
                {
                    ShadowCopyFiles = "true"
                });
                appDomain.ExecuteAssemblyByName(typeof(Program).Assembly.FullName, args);
                return;
            }

            Program2.ContinueMain(args);
            Console.ReadKey();
        }

    }
}
