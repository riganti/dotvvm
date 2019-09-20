using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Authentication
{
    public class RegisterPageObject : SeleniumHelperBase
    {
        public TextBoxProxy UserName
        {
            get;
        }

        public TextBoxProxy Password
        {
            get;
        }

        public TextBoxProxy ConfirmPassword
        {
            get;
        }

        public ValidationSummaryProxy ValidationSummary
        {
            get;
        }

        public ButtonProxy TextsLabel_Register
        {
            get;
        }

        public RouteLinkProxy Home
        {
            get;
        }

        public LinkButtonProxy Header_SignOut
        {
            get;
        }

        public RouteLinkProxy Header_SignIn
        {
            get;
        }

        public NestedUserControlPageObject NestedUserControl
        {
            get;
        }

        public RegisterPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            UserName = new TextBoxProxy(this, new PathSelector{UiName = "UserName", Parent = parentSelector});
            Password = new TextBoxProxy(this, new PathSelector{UiName = "Password", Parent = parentSelector});
            ConfirmPassword = new TextBoxProxy(this, new PathSelector{UiName = "ConfirmPassword", Parent = parentSelector});
            ValidationSummary = new ValidationSummaryProxy(this, new PathSelector{UiName = "ValidationSummary", Parent = parentSelector});
            TextsLabel_Register = new ButtonProxy(this, new PathSelector{UiName = "TextsLabel_Register", Parent = parentSelector});
            Home = new RouteLinkProxy(this, new PathSelector{UiName = "Home", Parent = parentSelector});
            Header_SignOut = new LinkButtonProxy(this, new PathSelector{UiName = "Header_SignOut", Parent = parentSelector});
            Header_SignIn = new RouteLinkProxy(this, new PathSelector{UiName = "Header_SignIn", Parent = parentSelector});
            NestedUserControl = new NestedUserControlPageObject(webDriver, this, new PathSelector{UiName = "NestedUserControl", Parent = parentSelector});
        }
    }
}