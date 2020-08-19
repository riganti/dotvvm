using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls
{
    public class UserControlPageObject : SeleniumHelperBase
    {
        public RouteLinkProxy TextsTitle_Detail
        {
            get;
        }

        public UserControlPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            TextsTitle_Detail = new RouteLinkProxy(this, new PathSelector{UiName = "TextsTitle_Detail", Parent = parentSelector});
        }
    }
}