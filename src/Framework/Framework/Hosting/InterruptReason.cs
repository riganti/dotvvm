namespace DotVVM.Framework.Hosting
{
    public enum InterruptReason
    {
        Unspecified = 0,
        Interrupt,
        Redirect,
        RedirectPermanent,
        ReturnFile,
        ModelValidationFailed,
        ArgumentsValidationFailed,
        CachedViewModelMissing,
        /// <summary> The request was rejected, most likely for security reasons. </summary>
        RequestRejected
    }
}
