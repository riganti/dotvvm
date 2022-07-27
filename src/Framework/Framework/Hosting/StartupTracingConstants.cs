using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting
{
    /// <summary>
    /// Constants containing names of events for tracing app startup.
    /// </summary>
    public class StartupTracingConstants
    {
        public static readonly string AddDotvvmStarted = nameof(AddDotvvmStarted);
        public static readonly string StartupComplete = nameof(StartupComplete);

        public static readonly string DotvvmConfigurationUserServicesRegistrationStarted = nameof(DotvvmConfigurationUserServicesRegistrationStarted);
        public static readonly string DotvvmConfigurationUserServicesRegistrationFinished = nameof(DotvvmConfigurationUserServicesRegistrationFinished);

        public static readonly string DotvvmConfigurationUserConfigureStarted = nameof(DotvvmConfigurationUserConfigureStarted);
        public static readonly string DotvvmConfigurationUserConfigureFinished = nameof(DotvvmConfigurationUserConfigureFinished);

        public static readonly string UseDotvvmStarted = nameof(UseDotvvmStarted);

        public static readonly string InvokeAllStaticConstructorsStarted = nameof(InvokeAllStaticConstructorsStarted);
        public static readonly string InvokeAllStaticConstructorsFinished = nameof(InvokeAllStaticConstructorsFinished);

        public static readonly string UseDotvvmFinished = nameof(UseDotvvmFinished);

        public static readonly string ViewCompilationStarted = nameof(ViewCompilationStarted);
        public static readonly string ViewCompilationFinished = nameof(ViewCompilationFinished);
    }
}
