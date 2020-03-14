namespace DotVVM.Testing.Abstractions
{
    public partial class SamplesRouteUrls
    {
        public static string FeatureSamples_RenderAdapter_Basic = "FeatureSamples/ControlRenderAdapters/BasicControlRenderAdapter";

        public const string FeatureSamples_PostbackConcurrency_DefaultMode =
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=Default";

        public const string FeatureSamples_PostbackConcurrency_QueueMode =
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=Queue";

        public const string FeatureSamples_PostbackConcurrency_DenyMode =
            "FeatureSamples/PostbackConcurrency/PostbackConcurrencyMode?concurrency=Deny";
        public const string FeatureSamples_Localization = "FeatureSamples/Localization";

        public const string Errors_Routing_NonExistingView = "Errors/Routing/NonExistingView";
        public const string FeatureSamples_PostBack_PostBackHandlers_Localized = "FeatureSamples/PostBack/PostBackHandlers_Localized";
        public const string ControlSamples_SpaContentPlaceHolder_HistoryApi = "ControlSamples/SpaContentPlaceHolder_HistoryApi";
    }
}
