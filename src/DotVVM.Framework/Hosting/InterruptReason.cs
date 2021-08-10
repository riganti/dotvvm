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
        CachedViewModelMissing
    }
}