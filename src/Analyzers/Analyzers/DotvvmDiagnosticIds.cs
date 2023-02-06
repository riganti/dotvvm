using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Analyzers
{
    public static class DotvvmDiagnosticIds
    {
        // Reserved for future use:
        public const string InternalDotvvmAnalyserError = "DotVVM01";

        public const string UseSerializablePropertiesInViewModelRuleId = "DotVVM02";
        public const string DoNotUseFieldsInViewModelRuleId = "DotVVM03";
        public const string DoNotInvokeMethodFromUnsupportedCallSiteRuleId = "DotVVM04";
    }
}
