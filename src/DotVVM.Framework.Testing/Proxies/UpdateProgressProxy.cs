namespace DotVVM.Framework.Testing.Proxies
{
    public class UpdateProgressProxy : WebElementProxyBase
    {
        public UpdateProgressProxy(SeleniumHelperBase helper, PathSelector selector) : base(helper, selector)
        {
        }

        public bool IsUpdateInProgress()
        {
            return FindElement().Displayed;
        }
    }
}
