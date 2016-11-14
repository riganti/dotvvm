namespace DotVVM.Framework.Hosting
{
    public class DotvvmEnvironmentNameProvider : IEnvironmentNameProvider
    {
        public string GetEnvironmentName(IDotvvmRequestContext context)
        {
            var owinContext = context.GetOwinContext();
            var environmentName = owinContext?.Get<string>(HostingConstants.HostAppModeKey);

            return string.IsNullOrWhiteSpace(environmentName)
                ? "Production"
                : environmentName;
        }
    }
}