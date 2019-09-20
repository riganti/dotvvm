using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls
{
    public class NestedUserControlPageObject : SeleniumHelperBase
    {
        public UserControlPageObject UserControl
        {
            get;
        }

        public NestedUserControlPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            UserControl = new UserControlPageObject(webDriver, this, new PathSelector{UiName = "UserControl", Parent = parentSelector});
        }
    }
}