using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.CRUD
{
    public class CreatePageObject : SeleniumHelperBase
    {
        public RouteLinkProxy GoBack
        {
            get;
        }

        public TextBoxProxy StudentFirstName
        {
            get;
        }

        public TextBoxProxy StudentLastName
        {
            get;
        }

        public TextBoxProxy StudentEnrollmentDate
        {
            get;
        }

        public TextBoxProxy StudentAbout
        {
            get;
        }

        public ButtonProxy TextsLabel_Add
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

        public CreatePageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            GoBack = new RouteLinkProxy(this, new PathSelector{UiName = "GoBack", Parent = parentSelector});
            StudentFirstName = new TextBoxProxy(this, new PathSelector{UiName = "StudentFirstName", Parent = parentSelector});
            StudentLastName = new TextBoxProxy(this, new PathSelector{UiName = "StudentLastName", Parent = parentSelector});
            StudentEnrollmentDate = new TextBoxProxy(this, new PathSelector{UiName = "StudentEnrollmentDate", Parent = parentSelector});
            StudentAbout = new TextBoxProxy(this, new PathSelector{UiName = "StudentAbout", Parent = parentSelector});
            TextsLabel_Add = new ButtonProxy(this, new PathSelector{UiName = "TextsLabel_Add", Parent = parentSelector});
            Home = new RouteLinkProxy(this, new PathSelector{UiName = "Home", Parent = parentSelector});
            Header_SignOut = new LinkButtonProxy(this, new PathSelector{UiName = "Header_SignOut", Parent = parentSelector});
            Header_SignIn = new RouteLinkProxy(this, new PathSelector{UiName = "Header_SignIn", Parent = parentSelector});
            NestedUserControl = new NestedUserControlPageObject(webDriver, this, new PathSelector{UiName = "NestedUserControl", Parent = parentSelector});
        }
    }
}