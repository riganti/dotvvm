using System;

namespace DotVVM.Compiler.Programs
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
            try
            {
#if NET461
                if (!AppDomain.CurrentDomain.ShadowCopyFiles)
                {
                    var appDomain = AppDomain.CreateDomain("SecondaryDomainShadowCopyAllowed", null, new AppDomainSetup {
                        ShadowCopyFiles = "true"
                    });
                    appDomain.ExecuteAssemblyByName(typeof(Program).Assembly.FullName, args);
                    return;
                }
#endif
                Program2.ContinueMain(args);
                Program2.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"!#" + e);
                Program2.Exit(1);
            }
        }
    }
}
