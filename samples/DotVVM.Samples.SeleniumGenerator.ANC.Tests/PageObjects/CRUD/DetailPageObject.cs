using DotVVM.Framework.Testing.SeleniumHelpers;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies;
using DotVVM.Framework.Testing.SeleniumHelpers.Proxies.GridViewColumns;
using DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.Controls;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Tests.PageObjects.CRUD
{
    public class DetailPageObject : SeleniumHelperBase
    {
        public RouteLinkProxy StudentList
        {
            get;
        }

        public LiteralProxy StudentEnrollmentDate
        {
            get;
        }

        public RouteLinkProxy TextsLabel_Edit
        {
            get;
        }

        public ButtonProxy TextsLabel_Delete
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

        public DetailPageObject(OpenQA.Selenium.IWebDriver webDriver, SeleniumHelperBase parentHelper = null, PathSelector parentSelector = null): base (webDriver, parentHelper, parentSelector)
        {
            StudentList = new RouteLinkProxy(this, new PathSelector{UiName = "StudentList", Parent = parentSelector});
            StudentEnrollmentDate = new LiteralProxy(this, new PathSelector{UiName = "StudentEnrollmentDate", Parent = parentSelector});
            TextsLabel_Edit = new RouteLinkProxy(this, new PathSelector{UiName = "TextsLabel_Edit", Parent = parentSelector});
            TextsLabel_Delete = new ButtonProxy(this, new PathSelector{UiName = "TextsLabel_Delete", Parent = parentSelector});
            Home = new RouteLinkProxy(this, new PathSelector{UiName = "Home", Parent = parentSelector});
            Header_SignOut = new LinkButtonProxy(this, new PathSelector{UiName = "Header_SignOut", Parent = parentSelector});
            Header_SignIn = new RouteLinkProxy(this, new PathSelector{UiName = "Header_SignIn", Parent = parentSelector});
            NestedUserControl = new NestedUserControlPageObject(webDriver, this, new PathSelector{UiName = "NestedUserControl", Parent = parentSelector});
        }
    }
}