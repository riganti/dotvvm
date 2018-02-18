namespace DotVVM.Testing.Abstractions
{
    public partial class SamplesRouteUrls
    {
        public static string FeatureSamples_PostbackConcurrency_NoneMode =>
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=None";

        public static string FeatureSamples_PostbackConcurrency_QueueMode =>
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=Queue";

        public static string FeatureSamples_PostbackConcurrency_DenyMode =>
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=Deny";
        public static string FeatureSamples_Localization => "FeatureSamples/Localization";

    }
}
