namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusPageApiOptions : StatusPageOptions
    {
        public NonAuthorizedApiAccessMode NonAuthorizedApiAccessMode { get; set; } = NonAuthorizedApiAccessMode.Deny;
        public new static StatusPageApiOptions CreateDefaultOptions()
        {
            return new StatusPageApiOptions()
            {
                RouteName = "StatusPageApi",
                Url = "_diagnostics/status/api"
            };
        }
    }
}